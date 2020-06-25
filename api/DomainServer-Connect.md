# DomainServer to MetaverseServer Connection handshake

This is a graphical capture of a DomainServer connecting to the MetaverseServer.

As of 20200521.

If DomainServer is a new DomainServer:

    DOMAINSERVER                                                METAVERSESERVER

Ask for identity     ------------------------------>
                       POST /api/v1/domains/temporary
                      <------------------------------           Create temp id,
                        200 OK                                   name, and access key
                         {
                           domain: {
                             id: "myNewDomainID",
                             name: "tinkle-winkle-4455",
                             api_key: "tempDomainAccessKey"
                           }
                         }

Tell server which ICE ------------------------------>
   server I'm using    PUT /api/v1/domains/{domainID}/ice_server_address
                          {
                           domain: {
                             ice_server_address: "0.0.0.0",
                             api_key: "tempDomainAccessKey"
                           }
                         }

Tell server public_key ------------------------------>
   for ICE server      PUT /api/v1/domains/{domainID}/public_key
   access                 multipart-form:
                           api_key: textTempDomainAccessKey
                           public_key: binaryPublicKey
                       <------------------------------
                        200 OK

                       ------------------------------>
                       GET /api/v1/domains/{domainID}
                       <------------------------------
                        200 OK
                          {
                           domain: {
                             id: "domainID"
                             ice_server_address: "192.168.1.444",
                             name: "placename"
                           }
                         }


If DomainServer is logged in TODO

<pre>

    DOMAINSERVER                                                METAVERSESERVER
                      ------------------------------>
Send "Hi" to server    PUT /api/v1/domains/{domainID}           Check if domainID is known
                         { version: "K2",
                           protocol: "lsdjkflksjflsj"
                           network_adress: "192.168.1.100",
                           automatic_networking: "full",
                           restricted: true,
                           api_key: "keyIfTempDomain",
                           heartbeat: {
                              num_anon_users: 0,
                              num_users: 0,
                           }
                      <------------------------------           Not known
                        404 NOT FOUND

Ask for temp identity ------------------------------>
                       POST /api/v1/domains/temporary
                      <------------------------------           Create temp id,
                        200 OK                                   name, and access key
                          { status: "success",
                            data: {
                              domain: {
                                id: newDomainIDguid,
                                name: newTempDomainName,
                                api_key: tempDomainAccessKey
                              }
                            }
                          }
Tell server ICE info  ------------------------------>
                       PUT /api/v1/domains/{domainID}/ice_server_address
                         {
                           domain: {
                             ice_server_address: "192.168.1.444",
                             api_key: "tempDomainAccessKey"
                           }
                         }
                      <------------------------------           Create temp id,
                        200 OK                                   name, and access key



</pre>

[Vircadia]: https://vircadia.com/
[HighFidelity]: https://en.wikipedia.org/wiki/High_Fidelity_(company)
