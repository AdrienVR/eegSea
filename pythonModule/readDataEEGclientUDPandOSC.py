# -*- coding: cp1252 -*-
# readDataEEGclientUDPandOSC.py
# open record4udp and records4osc and send udp and osc trame
# 1 : sends theta, alpha, beta and gamma values within an UDP socket
# 2 : sends normalised theta, alpha, beta and gamma values (in [0,1]) within an OSC socket

# lumière comme speed

import socket
import time
import os
import OSC


if __name__ == "__main__":
    # ouverture des fichiers d'enregistrements
    record4udp=open("record4udp.txt","r")
    udplines=record4udp.readlines()
    record4udp.close()
    record4osc=open("record4osc.txt","r")
    osclines=record4osc.readlines()
    record4osc.close()
    # ouverture de la socket UDP
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    UDP_IP = "127.0.0.1"    # adresse IP de la machine avec mer unity
    UDP_PORT = 5000         # port udp entre python et unity
    # ouverture de la socket OSC
    OSC_IP = "127.0.0.1"    # adresse IP de la machine avec le son
    OSC_PORT = 5001         # port OSC entre python et le son
    client = OSC.OSCClient()
    client.connect((OSC_IP,OSC_PORT))
    msg = OSC.OSCMessage()
    msg.setAddress("/test")
    # choix de la fréquence des envois udp
    udp_freq = 10           # (Hz) fréquence de l'envoi des trames udp et osc
    udp_period = 1./udp_freq
    # initialisation des données
    inittime=time.time()
    lasttime=inittime
    now=lasttime

    i_udp=0 # index pour les lignes udp
    i_osc=0 # index pour les lignes osc
    # max des différentes énergies (en log)
    try:
        while True:
            # maj du temps réel pour décider de l'envoi par udp
            now=time.time()
            if now-lasttime > udp_period :
                if i_udp+3>len(udplines) : i_udp=0 # on rembobine
                sock.sendto(udplines[i_udp],(UDP_IP,UDP_PORT))                
                sock.sendto(udplines[i_udp+1],(UDP_IP,UDP_PORT))
                sock.sendto(udplines[i_udp+2],(UDP_IP,UDP_PORT))
                sock.sendto(udplines[i_udp+3],(UDP_IP,UDP_PORT))
                i_udp+=4
                if i_osc>=len(osclines) : i_osc=0 # on rembobine
                msg.clearData()
                msg.append(osclines[i_osc])
                client.sendto(msg, (OSC_IP, OSC_PORT))
                if i_osc%10 == 0 : print 'at',now,'s',osclines[i_osc]
                i_osc+=1
                lasttime=now
            else : time.sleep(udp_period-(now-lasttime))
    except KeyboardInterrupt:
        headset.close()
        sock.close()
        client.close()
    finally:
        headset.close()
        sock.close()
        client.close()
