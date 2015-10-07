//Pour Ubuntu 14.04 AMD64

//Installer PIP :
sudo apt-get install python-dev python-pip
wget https://raw.githubusercontent.com/pypa/pip/master/contrib/get-pip.py
sudo python2.7 get-pip.py

//Installer les dépendances du pilote :
sudo pip install gevent
sudo pip install pycrypto
sudo pip install pywinusb

//Installer le pilote du casque EEG :
cd emokit-master/python
sudo python2.7 setup.py install

//Installer les dépendances du projet Python :
sudo pip install numpy
sudo pip install pyOSC --pre
sudo apt-get install realpath

//Autoriser l'utilisateur à utiliser le dongle USB.
//Créer le fichier :
sudo nano /etc/udev/rules.d/99-hidraw-permissions.rules
//Ajouter au fichier la ligne suivante :
KERNEL=="hidraw*", SUBSYSTEM=="hidraw", MODE="0664", GROUP="plugdev"
//Valider avec le raccourcis : CTRL+O, puis quitter avec CTRL+Q.
//Ajouter l'utilisateur au groupe plugdev :
sudo usermod -g plugdev {nom de l'utilisateur}
//Redémarrer

/opt/Unity/Editor/Unity OU optirun /opt/Unity/Editor/Unity pour les machines avec optimus configuré
python2.7 AquisitionEEGclientUDPandOSC.py pour transmettre le signal
