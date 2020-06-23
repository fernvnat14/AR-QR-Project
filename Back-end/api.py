from flask import Flask, request, jsonify, json
from flask_restful import Resource, Api
from flask_cors import CORS
from math import sin, cos, sqrt, atan2, radians
import random
#import urllib2.parse
import json
import requests
from flaskext.mysql import MySQL
app = Flask(__name__)
mysql = MySQL()
CORS(app)

app.config['MYSQL_DATABASE_USER'] = 'root'
app.config['MYSQL_DATABASE_PASSWORD'] = '12123'
app.config['MYSQL_DATABASE_DB'] = 'UnityDB'
app.config['MYSQL_DATABASE_HOST'] = '35.223.187.119'
mysql.init_app(app)

@app.route('/api/v1/addDatas', methods = ['POST'])
def posttoDB():
    con = mysql.connect()
    data = request.get_json()
    #print data
    if len(data) != 6:
        return {"Status":"Bad Request"}, 400
    cur = con.cursor()
    cur.execute("insert into socialData(ID, Name, QR_ID, Twitter, FB, IG, UserQuote,Username) values (null,%s,%s,%s,%s,%s,%s)",(data["Name"],data["QR_ID"],data["Twitter"],data["FB"],data["IG"],data["UserQuote"],data["Username"]))
    cur.execute("UPDATE socialData SET Name = REPLACE(Name,' ', '')")
    con.commit()
    return {"Successfully": "Registered"} , 201

@app.route('/api/v1/editCaption/<Username>', methods = ['POST'])
def editPlaces(Username=None):
    con = mysql.connect()
    data = request.get_json()
    if len(data) != 1:
        return {"Status":"Bad Request"}, 400
    cur = con.cursor()
    cur.execute("UPDATE userInfo SET caption = %s WHERE Username = %s;",(data["UserQuote"],Username))
    #cur.execute("UPDATE places SET Type = REPLACE(Type,' ', '')")
    con.commit()
    return {"Successfully": "Edited"} , 201

@app.route('/api/v1/viewall/qr/<QR_ID>')
def viewMeterByID(QR_ID=None):
    cur = mysql.connect().cursor()
    cur.execute("SELECT * FROM userInfo WHERE QR_ID = %s",QR_ID)
    data = [dict((cur.description[i][0], value)
              for i, value in enumerate(row)) for row in cur.fetchall()]
    return jsonify(data[0])

@app.route('/api/v1/viewall/username/<Username>')
def view(Username=None):
    cur = mysql.connect().cursor()
    cur.execute("SELECT * FROM userInfo WHERE Username = %s",Username)
    data = [dict((cur.description[i][0], value)
              for i, value in enumerate(row)) for row in cur.fetchall()]
    return jsonify(data[0])
    
@app.route('/api/v1/registerAccount', methods = ['POST'])
def postINFOtoDB():
    con = mysql.connect()
    data = request.get_json()
    if len(data) != 8:
        return {"Status":"Bad Request"}, 400
    cur = con.cursor()
    cur.execute("insert into userInfo(ID, Username, Password, Firstname, Lastname, QR_ID, Twitter, FB, IG, caption) values (null,%s,%s,%s,%s,%s,%s,%s,%s,null)",(data["Username"],data["Password"],data["Firstname"],data["Lastname"],data["QR_ID"],data["Twitter"],data["FB"],data["IG"]))
#    cur.execute("UPDATE userInfo SET Firstname = REPLACE(Firstname,' ', '')")
    con.commit()
    return {"Successfully": "Registered"} , 201
    
@app.route('/api/v1/Login', methods = ['POST'])
def idenLogin():
    con = mysql.connect()
    getdata = request.get_json()
    Username = getdata["Username"]
    Password = getdata["Password"]
    if len(getdata) != 2:
        return {"Status":"Bad Request"}, 400
    cur = con.cursor()
    cur.execute("SELECT * FROM userInfo WHERE Username = %s AND Password = %s",(Username,Password))
    data = [dict((cur.description[i][0], value)
              for i, value in enumerate(row)) for row in cur.fetchall()]
    row_count = len(data)
    if row_count == 1:
        return jsonify({"status":"Login successfully"}), 200
    else:
        return jsonify({"status":"Login failed"}), 401
#   return jsonify(data)
if __name__ == '__main__':
    app.run(host='0.0.0.0', port=8080, threaded=True, debug=True)


