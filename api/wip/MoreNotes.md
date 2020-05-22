# More Notes on Vircadia Protocols and APIs

If environment variable "HIFI_METAVERSE_URL" exists, use it or default to "https://metaverse.highfidelity.com"

Looks up 'ice.highfidelity.com' and 'stun.highfidelity.io'. Why two?

Settings parameter "oauth.provider" is OAUTH server used or default to METAVERSE_URL.
    Not sure where settings come from
        DomainServerSettingsManager::SETTING_DESCRIPTION_RELATIVE_PATH = "/resources/describe-settings.json"
    Environment variable: OAUTH_CLIENT_SECRET = client secret
        Or setting option "oauth.client-secret"


Ports
    40100: http
    40101: https
    40102:

    53210-5321?: assignment local ports


The server runs on 9400.
    Got "permission denied" when registering prefix
    20200518143103668,[MetaverseServer] Exception starting HttpListener:
        System.Net.HttpListenerException (5): Access is denied.
           at System.Net.HttpListener.AddPrefixCore(String registeredPrefix)
    Found https://stackoverflow.com/questions/4019466/httplistener-access-denied
        For Windows10, on admin shell:
            netsh http add urlacl url=https://+:9400/ user=everyone
    20200518155532183,[MetaverseServer] Exception starting HttpListener:
        System.Net.HttpListenerException (183): Failed to listen on prefix 'http://+:9400/'
            because it conflicts with an existing registration on the machine
    netsh http show urlacl
        show the existing listener reservations. 
    To remove an ACL reservation:
        netsh http delete urlacl url=https://+:9400/
    Tried adding "http://*:9400/api/" and got same "conflicting" error
    To list all the open ports:
        netstat -ano

