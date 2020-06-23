from functools import wraps
from flask import Flask, request, jsonify, json
from flask_restful import Resource, Api
from flask_cors import CORS
from flask_httpauth import HTTPBasicAuth
from flask_basicauth import BasicAuth
from math import sin, cos, sqrt, atan2, radians
from passlib.hash import sha256_crypt

import random
import unicodedata
import json
import requests
import flask
import uuid
import os
from datetime import datetime
from flaskext.mysql import MySQL
from flask_jwt_extended import (
    JWTManager, jwt_required, create_access_token,
    jwt_refresh_token_required, create_refresh_token,
    get_jwt_identity, get_jwt_claims, verify_jwt_in_request
)

app = Flask(__name__)
mysql = MySQL()
CORS(app)

app.config['MYSQL_DATABASE_USER'] = 'root'
app.config['MYSQL_DATABASE_PASSWORD'] = '12123'
app.config['MYSQL_DATABASE_DB'] = 'arnutrition'
app.config['MYSQL_DATABASE_HOST'] = '35.223.187.119'

app.config['JWT_SECRET_KEY'] = '8cda692c-20bb-452b-a762-4d341ea6c591'  # Change this!

jwt = JWTManager(app)
mysql.init_app(app)
auth = HTTPBasicAuth()

def dailyIntakeValue(value, name):
    if value is None:
        dv = 0
    elif name == "protein":
        dv = (value/50)*100
    elif name == "fat":
        dv = (value/70)*100
    elif name == "cholesterol":
        dv = (value/0.3)*100
    elif name == "sodium":
        dv = (value/2.4)*100
    elif name == "carbs":
        dv = (value/310)*100
    elif name == "sugars":
        dv = (value/90)*100
    return {"value": value, "dv": dv}


def dailyValue(value, name):
    if value is None:
        dv = 0
    elif name == "fat":
	dv = (value/65)*100
    elif name == "cholesterol":
	dv = (value/0.3)*100
    elif name == "sodium":
        dv = (value/2.4)*100
    elif name == "carbs":
        dv = (value/300)*100
    return {"value": value, "dv": dv}


def admin_required(fn):
    @wraps(fn)
    def wrapper(*args, **kwargs):
        verify_jwt_in_request()
        claims = get_jwt_claims()
        if claims['roles'] != 'Admin':
            return jsonify(msg='Admins only!'), 403
        else:
            return fn(*args, **kwargs)
    return wrapper


@jwt.user_claims_loader
def add_claims_to_access_token(identity):
    if identity == 'admin':
        return {'roles': 'Admin'}
    else:
        return {'roles': 'User'}


@auth.verify_password
def authenticate(username, password):
    if username and password:
        con = mysql.connect()
        cur = con.cursor()
        validUser_command = "SELECT COUNT(*), password FROM User WHERE username = %s GROUP BY password"
        cur.execute(validUser_command, username)
        data = [dict((cur.description[i][0], value)
                     for i, value in enumerate(row)) for row in cur.fetchall()]
        count = data[0].get("COUNT(*)")
        if count == 1:
	    password_hash = data[0].get("password")
	    if sha256_crypt.verify(password, password_hash):
                return True
	    else:
		return False
        else:
            return False
    return False


@app.route('/api/v1/refresh', methods=['POST'])
@jwt_refresh_token_required
def refresh():
    current_user = get_jwt_identity()
    ret = {
        'access_token': create_access_token(identity=current_user)
    }
    return jsonify(ret), 200


@app.route('/api/v1/login', methods=['GET'])
@auth.login_required
def get_response():
    con = mysql.connect()
    cur = con.cursor()
    username = auth.current_user()
    getName_command = "SELECT name FROM User WHERE username = %s"
    cur.execute(getName_command, username)
    data = [dict((cur.description[i][0], value)
                 for i, value in enumerate(row)) for row in cur.fetchall()]
    name = data[0].get("name")
    ret = {
        'access_token': create_access_token(identity=username),
        'refresh_token': create_refresh_token(identity=username),
        "username": username,
        "name": name
    }
    return jsonify(ret), 200


@app.route('/api/v1/register', methods=['POST'])
@admin_required
def register():
    con = mysql.connect()
    cur = con.cursor()
    json_body = request.get_json()
    validation_command = "SELECT COUNT(*) FROM User WHERE username = %s OR email =%s"
    cur.execute(validation_command, (json_body["username"], json_body["email"]))
    data = [dict((cur.description[i][0], value)
                 for i, value in enumerate(row)) for row in cur.fetchall()]
    count = data[0].get("COUNT(*)")
    if count == 0:
        try:
	    password_hash = sha256_crypt.encrypt(json_body["password"])
            register_command = "INSERT INTO User (username, password, name, email, type) VALUES (%s, %s, %s, %s, %s)"
            cur.execute(register_command, (
                json_body["username"], password_hash, json_body["name"], json_body["email"], json_body["type"]))
            con.commit()
            created_response = {'username': json_body["username"], 'name': json_body["name"],
                                'email': json_body["email"], 'type': json_body["type"], 'status': 'CREATED'}
            return jsonify(created_response), 201
        except Exception as e:
            response_status = {"status": "Problem inserting into db: " + str(e)}
            print(response_status)
            return jsonify(response_status), 500
    else:
        return jsonify("this username or email is already registered"), 409


@app.route('/api/v1/health', methods=['GET'])
@admin_required
def healthCheck():
    return jsonify({"status": "OK"})


@app.route('/api/v1/daily-intake', methods=['GET', 'POST'])
@jwt_required
def dailyIntake():
    con = mysql.connect()
    cur = con.cursor()
    now = datetime.now()
    username = get_jwt_identity()
    #username = "pigboss1"
    dt_string = now.strftime("%d/%m/%Y")
    if flask.request.method == 'POST':
        json_body = request.get_json()
        if json_body["username"] == username:
            addHistory_command = "INSERT INTO DailyIntake (username, calories, protein, fat, carbs, sugars, sodium, cholesterol,date) VALUES(%s, %s, %s, %s, %s, %s, %s, %s, %s)"
            addProductEat_command = "INSERT INTO ProductEat (username, product_id, date) VALUES(%s, %s, %s)"
            try:
                cur.execute(addHistory_command, (
                    json_body["username"], json_body["calories"], json_body["protein"], json_body["fat"],
                    json_body["carbs"], json_body["sugars"], json_body["sodium"], json_body["cholesterol"], dt_string))
                cur.execute(addProductEat_command, (json_body["username"], json_body["product_id"], dt_string))
                con.commit()
                return {'status': 'Information added', 'date': dt_string}, 201
            except Exception as e:
                response_status = {"status": "Problem inserting into db: " + str(e)}
                return jsonify(response_status), 500
        return jsonify("You don't have permission to accessing this information."), 401

    if flask.request.method == 'GET':
        request_username = request.args.get('username')
        if request_username == username:
            if username is None or username == '':
                return "Please add required query params", 400
            # getDailyIntake_command = "SELECT * FROM DailyIntake WHERE username = %s AND date = %s"
            getDailyIntake_command = "SELECT username, SUM(calories) as calories, SUM(protein) as protein, SUM(fat) as fat, SUM(carbs) as carbs, SUM(sugars) as sugars, SUM(sodium) as sodium, SUM(cholesterol) as cholesterol, date FROM DailyIntake WHERE username = %s AND date = %s"
            try:
                cur.execute(getDailyIntake_command, (username, dt_string))
                data = [dict((cur.description[i][0], value)
                             for i, value in enumerate(row)) for row in cur.fetchall()]
                dailyIntake = {"calories": data[0]["calories"], "carbs": dailyIntakeValue(data[0]["carbs"], "carbs"),
                               "cholesterol": dailyIntakeValue(data[0]["cholesterol"], "cholesterol"),"fat": dailyIntakeValue(data[0]["fat"], "fat"),
                               "protein": dailyIntakeValue(data[0]["protein"], "protein"), "sodium": dailyIntakeValue(data[0]["sodium"], "sodium"), "sugars": dailyIntakeValue(data[0]["sugars"], "sugars"), 
                               "date": dt_string, "username": username}
                return jsonify(dailyIntake), 200
            except Exception as e:
                response_status = {"status": "Problem request data from  db: " + str(e)}
                return jsonify(response_status), 500
        return jsonify("You don't have permission to accessing this information."), 401


@app.route('/api/v1/product-eat-daily', methods=['GET'])
@jwt_required
def productEat():
    con = mysql.connect()
    cur = con.cursor()
    username = get_jwt_identity()
    request_username = request.args.get('username')
    date = request.args.get('date')
    if request_username == username:
        getProductEat_command = "SELECT Product.product_id, Product.product_name, ProductEat.username, ProductEat.date FROM Product INNER JOIN ProductEat ON Product.product_id=ProductEat.product_id WHERE ProductEat.username = %s AND date = %s"
        try:
            cur.execute(getProductEat_command, (username, date))
            data = [dict((cur.description[i][0], value)
                         for i, value in enumerate(row)) for row in cur.fetchall()]
            return jsonify(data), 200
        except Exception as e:
            response_status = {"status": "Problem request data from  db: " + str(e)}
            return jsonify(response_status), 500
    return jsonify("You don't have permission to accessing this information."), 401


@app.route('/api/v1/generating-uuid', methods=['GET'])
@admin_required
def generatingUUID():
    id = uuid.uuid4()
    con = mysql.connect()
    cur = con.cursor()
    sql_command = "SELECT COUNT(*) FROM Product WHERE product_id = %s"
    cur.execute(sql_command, str(id))
    data = [dict((cur.description[i][0], value)
                 for i, value in enumerate(row)) for row in cur.fetchall()]
    count = data[0].get("COUNT(*)")
    if count == 0:
        response_data = {"uuid": str(id)}
        return jsonify(response_data), 200
    else:
        return generatingUUID()


@app.route('/api/v1/allproduct', methods=['GET'])
@jwt_required
def allProduct():
    con = mysql.connect()
    cur = con.cursor()
    username = get_jwt_identity()
    cur.execute(
        "SELECT UserWithProduct.username, UserWithProduct.name, Product.calories, Product.carbs, Product.cholesterol, Product.fat, Product.product_id, Product.product_name, Product.protein, Product.sodium, Product.sugars, Product.uuid FROM Product JOIN UserWithProduct ON Product.product_id = UserWithProduct.product_id WHERE UserWithProduct.username = %s",
        username)
    data = [dict((cur.description[i][0], value)
                 for i, value in enumerate(row)) for row in cur.fetchall()]
    return jsonify(data), 200


@app.route('/api/v1/product', methods=['GET', 'POST'])
@admin_required
def viewNutritionByID():
    con = mysql.connect()
    cur = con.cursor()
    if flask.request.method == 'POST':
        json_body = request.get_json()
        if len(json_body) != 12:
            return {"Status": "Bad Request"}, 400
        sql_command_nutrition = "INSERT INTO Product (product_id, product_name, calories, protein, fat, carbs, sugars, sodium, cholesterol, uuid) VALUES(%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"
        sql_command_nutrition_with_user = "INSERT INTO UserWithProduct (username, name, product_id, product_name) VALUES(%s, %s, %s, %s)"
        try:
            cur.execute(sql_command_nutrition, (
                json_body["product_id"], json_body["product_name"], json_body["calories"], json_body["protein"],
                json_body["fat"], json_body["carbs"], json_body["sugars"], json_body["sodium"],
                json_body["cholesterol"],
                json_body["uuid"]))
            cur.execute(sql_command_nutrition_with_user,
                        (json_body["username"], json_body["name"], json_body["product_id"], json_body["product_name"]))
            con.commit()
            response_status = {"product_id": json_body["product_id"], "product_name": json_body["product_name"],
                               "status": "CREATED"}
            return jsonify(response_status), 201
        except Exception as e:
            response_status = {"status": "Problem inserting into db: " + str(e)}
            return jsonify(response_status), 500

    elif flask.request.method == 'GET':
        product_id = request.args.get('product_id')
        cur.execute("SELECT Product.product_id, Product.product_name, Product.calories, Product.protein, Product.fat, Product.carbs, Product.sugars, Product.sodium, Product.cholesterol, UserWithProduct.username FROM Product JOIN UserWithProduct ON Product.product_id = UserWithProduct.product_id WHERE Product.product_id = %s;", product_id)
        data = [dict((cur.description[i][0], value)
                     for i, value in enumerate(row)) for row in cur.fetchall()]
	response = {"product_name":data[0]["product_name"], "product_id": data[0]["product_id"], "calories": data[0]["calories"], "carbs": dailyValue(data[0]["carbs"], "carbs"),
                    "cholesterol": dailyValue(data[0]["cholesterol"], "cholesterol"),"fat": dailyValue(data[0]["fat"], "fat"),
                    "protein": data[0]["protein"], "sodium": dailyValue(data[0]["sodium"], "sodium"), "sugars": data[0]["sugars"], "username": data[0]["username"]}
        return jsonify(response), 200


@app.route('/api/v1/product/<product_id>', methods=['PATCH', 'DELETE'])
@jwt_required
def nutrition_ID(product_id=None):
    con = mysql.connect()
    cur = con.cursor()
    checkOwner_command = "SELECT username FROM UserWithProduct WHERE product_id = %s"
    cur.execute(checkOwner_command, product_id)
    data = [dict((cur.description[i][0], value)
                 for i, value in enumerate(row)) for row in cur.fetchall()]
    product_owner = data[0].get("username")
    username = get_jwt_identity()
    #username = "pigboss1"
    if product_owner == username:
        if len(data) == 0:
            return jsonify("Invalid Product"), 404
        elif flask.request.method == 'DELETE':
            sql_command = 'DELETE FROM Product WHERE product_id = %s'
	    sql_command_2 = 'DELETE FROM UserWithProduct WHERE product_id = %s'
            try:
                cur.execute(sql_command, product_id)
		cur.execute(sql_command_2, product_id)
                con.commit()
                return jsonify({'product_id': product_id, 'status': 'Resource already deleted.'}), 200
            except Exception as e:
                return jsonify({'product_id': product_id, 'status': 'Delete unsuccessful.'}), 500
        elif flask.request.method == 'PATCH':
            now = datetime.now()
            dt_string = now.strftime("%d/%m/%Y")
            json_body = request.get_json()
            if json_body['update_type'] == 'nutrition labels':
                sql_command = 'UPDATE Product SET calories=%s, protein=%s, fat=%s, carbs=%s, sugars=%s, sodium=%s, cholesterol=%s, updated_at = %s WHERE product_id=%s'
                try:
                    cur.execute(sql_command, (
                        json_body['calories'], json_body['protein'], json_body['fat'], json_body['carbs'],
                        json_body['sugars'], json_body['sodium'], json_body['cholesterol'], dt_string, json_body['product_id']))
                    con.commit()
                    return jsonify({"status": "UPDATED"}), 200
                except Exception as e:
                    response_status = {"status": "Problem inserting into db: " + str(e)}
                    return jsonify(response_status), 500
            elif json_body['update_type'] == 'calories':
                sql_command = 'UPDATE Product SET calories=%s, updated_at = %s WHERE product_id=%s'
                try:
                    cur.execute(sql_command, (json_body['calories'], dt_string, json_body['product_id']))
                    con.commit()
                    return jsonify({"status": "UPDATED"}), 200
                except Exception as e:
                    response_status = {"status": "Problem inserting into db: " + str(e)}
                    return jsonify(response_status), 500
            elif json_body['update_type'] == 'product_name':
                sql_command = 'UPDATE Product SET product_name=%s, updated_at = %s WHERE product_id=%s'
                try:
                    cur.execute(sql_command, (json_body['product_name'], dt_string, json_body['product_id']))
                    con.commit()
                    return jsonify({"status": "UPDATED"}), 200
                except Exception as e:
                    response_status = {"status": "Problem inserting into db: " + str(e)}
                    return jsonify(response_status), 500
    return jsonify("You don't have permission to accessing this information."), 401


if __name__ == '__main__':
    app.run(threaded=True, debug=True)
