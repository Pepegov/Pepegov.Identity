#!/usr/bin/env bash
set -eu
org=localhost-ca
domain=localhost

sudo mkdir -p /opt/OpendIddict
sudo mkdir -p /opt/OpendIddict/cents
sudo chown "$USER":users /opt/OpendIddict/cents

openssl genpkey -algorithm RSA -out /opt/OpendIddict/cents/ca.key
openssl req -x509 -key /opt/OpendIddict/cents/ca.key \
	-out /opt/OpendIddict/cents/ca.crt \
    	-subj "/CN=$org/O=$org"

openssl genpkey -algorithm RSA -out /opt/OpendIddict/cents/"$domain".key
openssl req -new -key /opt/OpendIddict/cents/"$domain".key \
	-out /opt/OpendIddict/cents/"$domain".csr \
    	-subj "/CN=$domain/O=$org"

openssl x509 -req \
	-in /opt/OpendIddict/cents/"$domain".csr -days 365 \
	-out /opt/OpendIddict/cents/"$domain".crt \
    	-CA /opt/OpendIddict/cents/ca.crt \
	-CAkey /opt/OpendIddict/cents/ca.key \
	-CAcreateserial \
    	-extfile <(cat <<END
basicConstraints = CA:FALSE
subjectKeyIdentifier = hash
authorityKeyIdentifier = keyid,issuer
subjectAltName = DNS:$domain
END
    )

openssl pkcs12 -export -out /opt/OpendIddict/cents/"$domain".pfx \
	-inkey /opt/OpendIddict/cents/"$domain".key \
	-in /opt/OpendIddict/cents/"$domain".crt

sudo trust anchor /opt/OpendIddict/cents/ca.crt 
