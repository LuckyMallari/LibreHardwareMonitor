#!/usr/bin/env python3
import bluetooth
import threading
import os

from flask import Flask, make_response
app = Flask(__name__)
payload = ""

@app.route('/metrics')
def metrics():
    global payload
    response = make_response(payload, 200)
    response.mimetype = "text/plain"
    return response
 
def rfcommServer(): 
    global payload
    
    print("Turning on BT scan..")
    os.system('hciconfig hci0 piscan')

    server_sock = bluetooth.BluetoothSocket(bluetooth.RFCOMM)
    server_sock.bind(("", bluetooth.PORT_ANY))
    server_sock.listen(1)
    port = server_sock.getsockname()[1]
    uuid = "94f39d29-7d6d-437d-973b-fba39e49d4ee"

    bluetooth.advertise_service(server_sock, "SampleServer", service_id=uuid,
                                service_classes=[uuid, bluetooth.SERIAL_PORT_CLASS],
                                profiles=[bluetooth.SERIAL_PORT_PROFILE]
                                )

    while True:
        print("Waiting for connection on RFCOMM channel", port)
        try:
            client_sock, client_info = server_sock.accept()
        except:
            pass
            
        print("Accepted connection from", client_info)
        currentData = ""
        
        try:
            while True:
                data = client_sock.recv(1024)
                if not data:
                    break
                    
                if data.decode("utf-8") == "###<SHUTDOWN>":
                    os.system("sudo poweroff")
                    print("Received poweroff. Shutting down...")
                    return
                
                currentData += data.decode("utf-8")
                if "<END>" in currentData:
                    payload = '%s' % currentData  
                    currentData = ""
                    print("Received..")

        except OSError:
            pass

        print("Disconnected.")
        client_sock.close()
        server_sock.close()

threading.Thread(target=rfcommServer).start()

if __name__ == '__main__':
    app.run(debug=False, host='0.0.0.0')    

print("All done.")

