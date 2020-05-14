# Login-Logout Handshake

Based on a .har file captured from a [Vircadia] login/logout,
this is the communication.
This is the legacy handshake as inherited from [HighFidelity]
and will change.

All the user names, passwords, keys, and certificates have been messed with so
they are not the original values.

As of 20200514.

<pre>

Interface                                           DomainServer
PUT https://DOMAINSERVER/api/v1/usr/heartbeat
    Header: HFM-SessionID: 93fbc10e-626c-4194-9b95-96ce3980aec8
    Body: ""
    Response: 200 
    Header: HFM-SessionID: 93fbc10e-626c-4194-9b95-96ce3980aec8
        Server: nginx/1.10.3 (Ubuntu)
    Body:
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"}}"

GET https://DOMAINSERVER/api/v1/commerce/marketplace_key
    Header: no "HFM-SessionID" header
    Response: 200
    Header:
        Server: nginx/1.10.3 (Ubuntu)
    Body:
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"public_key\":\"-----BEGIN PUBLIC KEY-----\\nMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEzAV1CqdfoCpw65jNMoLxnrRF+FsC\\nxeeBR/vOJ89358y2Ew/kDq1Wa4m1cot1Qp/ojSj8itEVY/XpzYpNqOys6w==\\n-----END PUBLIC KEY-----\\n\"}}"

GET https://DOMAINSERVER/api/v1/commerce/marketplace_key
    Header: no "HFM-SessionID" header
    Response: 200
    Header:
        Server: nginx/1.10.3 (Ubuntu)
    Body:
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"public_key\":\"-----BEGIN PUBLIC KEY-----\\nMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEzAV1CqdfoCpw65jNMoLxnrRF+FsC\\nxeeBR/vOJ89358y2Ew/kDq1Wa4m1cot1Qp/ojSj8itEVY/XpzYpNqOys6w==\\n-----END PUBLIC KEY-----\\n\"}}"

PUT https://DOMAINSERVER/api/v1/user/public_key
    Header:
        "HFM-SessionID": "93fbc10e-626c-4194-9b95-96ce3980aec8"
        "Content-Type": "multipart/form-data; boundary=\"boundary_.oOo._csZyP8PIPiY2DJiTssHzdDWuqmztlxhL\""`
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    Body:
        "mimeType": "multipart/form-data; boundary=\"boundary_.oOo._csZyP8PIPiY2DJiTssHzdDWuqmztlxhL\"",
       "text": "--boundary_.oOo._csZyP8PIPiY2DJiTssHzdDWuqmztlxhL\r\nContent-Type: application/octet-stream\r\nContent-Disposition: form-data; name=\"public_key\"; filename=\"public_key\"\r\n\r\n0\u0082\u0001\n\u0002\u0082\u0001\u0001\u0000\u00d3Y\u008a\u00a8\u00d3\u0084\u00bd\u008a\u00fd\u00a4\u00d7a\u00d5\u00f0x\u00c3\u00ffn\u00ed\u00c3\u00c8[\u00d7\u00ab\u00e1\u0093\u00a1\u009e(\u0019\u00ca\u00c3L\u00e7\u00eb\u008e-\r\u009b\u00cf&\u00c4\u00be\u00cel\u0016\u00ec\u0003\u00d1E\u00fef\u0091\u0083v\u008c\u00b7\u00c3QSX\u00bcg9\"\u00ae\u0017\u00a2g\u0005\u00c1\u00a5r\u00bd\n+\u0080\u00e0\u00e9\u00e9\u0013\u00e5\u0081\u0092\u00e2'\u00bc\u00d1\u00a3:\u0004\u0094\u00ab>M\u0080\"\u0017K\u0088JQt\u00c4/\u00b6\u00f5D\u0091\u00e2\u009c\u00b7\u00da\u00ea.&\u00c7\u00a3\u00b2\u009dL9\u001e\u0005\u00f9\u00f2Q\u00b2(Y\u00a3\u00b0\u00bf`\u00a1\u00e6SDM\u00cc\u00b4)\u00df\u00d3\u0082\u00d1\u00bf9\u00c9\"\u0095\u00ca\u00c4=\u00da\u00e2\u0099YY\u0011\u00d2>\u00ca\n\u00c2\u0015\u0013\u0083\u00cc^\"\u00ae@\u008d\u00ccFEx\u00a2\u00a4\u0097E\u008b\u0015;N\u00ee\u00e0\u00f5E\u00bc\u000eF\u0094\u0016\u0093\u00b9\u0003\u0011$\u00b0C?\u00fe\t\u00b3K\u008f\u00db\u00ab\u00f6v\u0001L\u0012q\u00df-\u00f1\u008dZq\u00c0m\u000b\u00a9\u0003O\u00e4\u00d5\u00a7\u0082\u00ad\u00e0\u00dd\u00b4\u009e\u00cd\u00cf\u009b\u00ae\u00dcr&gME;\u00d3g\u00fb\u00b2\u0097\u0013\u00b9\u007f\u0002\u0003\u0001\u0000\u0001\r\n--boundary_.oOo._csZyP8PIPiY2DJiTssHzdDWuqmztlxhL--\r\n",
    Response; 401 (unauthorized)
    Header:
        ": "WWW-Authenticate": "Bearer realm=\"Doorkeeper\", error=\"invalid_token\", error_description=\"The access token is invalid\""
    Body:
        "text": ""

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "HFM-SessionID": "93fbc10e-626c-4194-9b95-96ce3980aec8"
    Body:
        "mimeType": "application/json",
        "text": ""
        ""
    Response: 200
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"}}"

... several more 'heartbeat's

POST https://DOMAINSERVER/oauth/token
    "postData": {
        "mimeType": "application/x-www-form-urlencoded",
        "text": "grant_type=password&username=USER.NAME&password=XXXXXXXX&scope=owner",
        "params": [
            { "name": "grant_type", "value": "password" },
            { "name": "username", "value": "USER.NAME" },
            { "name": "password", "value": "XXXXXXXX" },
            { "name": "scope", "value": "owner" }
        ]
      }
      Response: 200 OK
          "mimeType": "application/json; charset=utf-8",
          "text": "{\"access_token\":\"5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13\",\"token_type\":\"Bearer\",\"expires_in\":2629746,\"refresh_token\":\"39d8ad8a371e4b53eb14ab272e8ba8afb69d4a1e7d8092ccff0371e677102cbc\",\"scope\":\"owner\",\"created_at\":1577652284}"

GET https://DOMAINSERVER/api/v1/user/profile
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    Body size zero
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"user\":{\"username\":\"USER.NAME\",\"roles\":[]}}}"

PUT https://DOMAINSERVER/api/v1/user/public_key
    "postData": {
      "mimeType": "multipart/form-data; boundary=\"boundary_.oOo._UZ2bD6g5jdkwSeIE9uh8J1lYoniGxero\"",
      "text": "--boundary_.oOo._UZ2bD6g5jdkwSeIE9uh8J1lYoniGxero\r\nContent-Type: application/octet-stream\r\nContent-Disposition: form-data; name=\"public_key\"; filename=\"public_key\"\r\n\r\n0\u0082\u0001\n\u0002\u0082\u0001\u0001\u0000\u00d2Y\u008a\u00a8\u00d3\u00e4\u00bd\u008a\u00fd\u00a4\u00d7a\u00d5\u00f0x\u00b3\u00ffn\u00ed\u00c3\u00c8[\u00d7\u00ab\u00e1\u0093\u00a1\u009e(\u0019\u00ca\u00c3L\u00f7\u00eb\u008e-\r\u009b\u00cf&\u00c4\u00be\u00cel\u0016\u00e8\u0003\u00d1E\u00fef\u0061\u0083v\u008c\u00a7\u00c3QSX\u00bcg9\"\u00ae\u0017\u00a2g\u0005\u00c1\u00a5r\u00bd\n+\u0080\u00e0\u00e9\u00e9\u0013\u00e5\u0081\u0092\u00e2'\u00bc\u00d1\u00a3:\u0004\u0094\u00ab>M\u0082\"\u0017K\u0088JQt\u00c4/\u00b6\u00f5D\u0092\u00e2\u009c\u00b7\u00da\u00ea.&\u00c7\u00a3\u00b2\u009dL9\u001e\u0005\u00f9\u00f2Q\u00b2(Y\u00a3\u00b0\u00bf`\u00a1\u00e6SDM\u00cc\u00b4)\u00df\u00d3\u0082\u00d1\u00bf9\u00c9\"\u0095\u00ca\u00c4=\u00da\u00e2\u0099YY\u0011\u00d2>\u00ca\n\u00c2\u0015\u0013\u0083\u00cc^\"\u00ae@\u008d\u00ccFEx\u00a2\u00a4\u0097E\u008b\u0015;N\u00ee\u00e0\u00f5E\u00bc\u000eF\u0094\u0016\u0093\u00b9\u0003\u0011$\u00b0C?\u00fe\t\u00b3K\u008f\u00db\u00ab\u00f6v\u0001L\u0012q\u00df-\u00f1\u008dZq\u00c0m\u000b\u00a9\u0003O\u00e4\u00d5\u00a7\u0082\u00ad\u00e0\u00dd\u00b4\u009e\u00cd\u00cf\u009b\u00ae\u00dcr&gME;\u00d3g\u00fb\u00b2\u0097\u0013\u00b9\u007f\u0002\u0003\u0001\u0000\u0001\r\n--boundary_.oOo._UZ2bD6g5jdkwSeIE9uh8J1lYoniGxero--\r\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\"}

GET https://DOMAINSERVER/api/v1/user/locker
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    Body size zero
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{}}"

PUT https://DOMAINSERVER/api/v1/commerce/hfc_account
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "size": 11015,
        "mimeType": "application/json; charset=utf-8"
            "text": "{\"status\":\"success\",
            \"data\":{
                \"salt\":\"rrHLyODXIUzqr\\t2.2h9fesF3p8c=\",
                \"public_key\":\"MFYwEAYHKoZIzj0CAQYFK4EEdkszQgAEtTfx92VtNepq9T5udl2hg3/McX49hh5bwer1QmGjgtRRhRZ/6w12xYq6mpWEE4In01HtQw/KMF+iH5R8PnWkAQ==\",
                \"iv\":\"gMBvdkxciMbJ0w7HG4yyDQ==\",
                \"ckey\":\"EReWWUxrksdczBhYoerN1eS9kvFqddaQMFqDK2UmWL0=\",
                \"locker\":\"-----BEGIN PUBLIC KEY-----\\nMFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEtTfxs9dtNepq9T5f782hg3/McX49hh5b\\nwer1QmGjgtRRhRZ/6w12xYq6mpWEE4In01HtQw/KMF+iH5R8PnWkAQ==\\n-----END PUBLIC KEY-----\\n
                -----BEGIN EC PRIVATE KEY-----\\nProc-Type: 4,ENCRYPTED\\nDEK-Info: DES-EDE3-CBC,BDD3AEE4CD19983B\\n\\nEognuG0zErMQI3xrle+sas8dlMrHHjL4bPEIlC7scZ/jJon2NbjEpiamS1Yeemwx\\nPTLXMVjDe97JMgi84zREbJ17P63DP4wS2iva4HglR1W+9q2+/9cmouQB7iXyte4a\\ndW3SxhVqBhGMnOYSRh/AVPKhBLd4tiWz\\n-----END EC PRIVATE KEY-----
                \\n-----BEGIN SECURITY IMAGE-----\\n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\\nZhUe8RubbM4=\\n-----END SECURITY IMAGE-----\\n\"}}"

GET https://DOMAINSERVER/api/v1/users?filter=connections&status=online&per_page=10
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    Body size zero
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"current_page\":1,\"total_pages\":1,\"per_page\":10,\"total_entries\":0,\"data\":{\"users\":[]}}"

GET https://DOMAINSERVER/api/v1/user_stories?include_actions=announcement&restriction=open,hifi&require_online=true&protocol=Qa12Qv2cYYxmNV8c%2Fqlf5A%3D%3D&per_page=10
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    Body size zero
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"current_page\":1,\"total_pages\":1,\"per_page\":10,\"total_entries\":0,\"user_stories\":[]}"

PUT https://DOMAINSERVER/api/v1/commerce/hfc_account
    "postData":
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "size": 11015,
        "mimeType": "application/json; charset=utf-8"
            "text": "{\"status\":\"success\",
            \"data\":{
                \"salt\":\"rrHLyODXIUzqr\\t2.2h9fesF3p8c=\",
                \"public_key\":\"MFYwEAYHKoZIzj0CAQYFK4EEdkszQgAEtTfx92VtNepq9T5udl2hg3/McX49hh5bwer1QmGjgtRRhRZ/6w12xYq6mpWEE4In01HtQw/KMF+iH5R8PnWkAQ==\",
                \"iv\":\"gMBvdkxciMbJ0w7HG4yyDQ==\",
                \"ckey\":\"EReWWUxrksdczBhYoerN1eS9kvFqddaQMFqDK2UmWL0=\",
                \"locker\":\"-----BEGIN PUBLIC KEY-----\\nMFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEtTfxs9dtNepq9T5f782hg3/McX49hh5b\\nwer1QmGjgtRRhRZ/6w12xYq6mpWEE4In01HtQw/KMF+iH5R8PnWkAQ==\\n-----END PUBLIC KEY-----\\n
                -----BEGIN EC PRIVATE KEY-----\\nProc-Type: 4,ENCRYPTED\\nDEK-Info: DES-EDE3-CBC,BDD3AEE4CD19983B\\n\\nEognuG0zErMQI3xrle+sas8dlMrHHjL4bPEIlC7scZ/jJon2NbjEpiamS1Yeemwx\\nPTLXMVjDe97JMgi84zREbJ17P63DP4wS2iva4HglR1W+9q2+/9cmouQB7iXyte4a\\ndW3SxhVqBhGMnOYSRh/AVPKhBLd4tiWz\\n-----END EC PRIVATE KEY-----
                \\n-----BEGIN SECURITY IMAGE-----\\n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\\nZhUe8RubbM4=\\n-----END SECURITY IMAGE-----\\n\"}}"

PUT https://DOMAINSERVER/api/v1/user/location
    "postData": {
        "mimeType": "application/json",
        "text": "{\n    \"location\": {\n        \"availability\": \"connections\",\n        \"connected\": false,\n        \"network_address\": \"\",\n        \"network_port\": 40102,\n        \"node_id\": \"00000000-0000-0000-0000-000000000000\",\n        \"path\": \"/0,-0.184357,0/0,0,0,1\"\n    }\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"}}"

GET https://DOMAINSERVER/api/v1/commerce/available_updates?per_page=10
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    Body size zero
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",
                \"current_page\":1,
                \"per_page\":10,
                \"total_entries\":1,
                \"total_pages\":1,
                \"data\":{
                    \"updates\":[
                        {\"item_id\":\"80946b87-2441-437c-8d72-48ddeb2a0493\",
                            \"updated_item_id\":\"1456c6f8-525a-430b-9217-5d6b39a8a874\",
                            \"base_item_title\":\"Blueprint: DANCE!\",
                            \"certificate_id\":\"MEUCIGTMLLCIQWcsewokYlMTpFLqYD4vSdHgqlgj/o6DVGAUAiEAyPnzs1IN40ikybUv1xCr0XbMn3GofpPXfswuy0aRSzA=\",
                            \"edition_number\":23,
                            \"total_entries\":1,
                            \"item_download_url\":\"https://metaverse.highfidelity.com/api/v1/commerce/entity_edition/80946b87-2441-437c-8d72-48ddeb2a0493.app.json?certificate_id=MEUCIGTMvJVmgm53%2BH%2FbYlMTpFLskc0wSdHgqlgj%2Fo6DVGAUAiEAyPnzs1IN40ikybUv1xCr0XbMn3GofpPXfswuy0aRSzA%3D\"
                        }
                    ]
                }
                }"

GET http://52.216.27.70/hifi-public/images/tools/progress-background.svg
    Headers:
        "Host": "s3.amazonaws.com"
    Response: 200 OK
        "mimeType": "image/svg+xml",
        "text": "\u00ef\u00bb\u00bf<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">\r\n<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" version=\"1.1\" baseProfile=\"full\" width=\"360\" height=\"120\" viewBox=\"0 0 360.00 120.00\" enable-background=\"new 0 0 360.00 120.00\" xml:space=\"preserve\">\r\n\t<path fill=\"#343434\" fill-opacity=\"1\" stroke-linejoin=\"round\" d=\"M 10,7.62939e-006L 350,7.62939e-006C 355.523,7.62939e-006 360,4.47717 360,10L 360,110C 360,115.523 355.523,120 350,120L 10,120C 4.47716,120 7.66041e-006,115.523 7.66041e-006,110L 7.66041e-006,10C 7.66041e-006,4.47717 4.47716,7.62939e-006 10,7.62939e-006 Z \"/>\r\n</svg>\r\n"

GET https://DOMAINSERVER/api/v1/commerce/history?per_page=10&since=1577651216.513
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    Body size zzero
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"history\":[]}}"

GET https://DOMAINSERVER/api/v1/commerce/available_updates?per_page=10&page=2
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    Body size zzero
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"current_page\":2,\"per_page\":10,\"total_entries\":0,\"total_pages\":1,\"data\":{\"updates\":[]}}"

GET https://DOMAINSERVER/api/v1/places/thepalace
    no Authroization header
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",
        \"data\":{
            \"place\":{
                \"id\":\"fb029858-8d3c-4581-83d2-2e254332fc92\",
                \"description\":\"A community build zone for those interested in working together.\",
                \"path\":\"/0.0489895,-11.4964,10.1377/0,0.0498893,0,0.998755\",
                \"name\":\"ThePalace\",
                \"address\":\"hifi://ThePalace/0.0489895,-11.4964,10.1377/0,0.0498893,0,0.998755\",
                \"domain\":{
                    \"id\":\"b7a3ac63-110b-491e-b8e6-5b39a0f613ca\",
                    \"network_address\":\"159.203.67.226\",
                    \"network_port\":40102,
                    \"cloud_domain\":true,
                    \"online\":true,
                    \"default_place_name\":\"ThePalace\"
                },
                \"previews\":{
                    \"thumbnail\":\"https://hifi-metaverse.s3-us-west-1.amazonaws.com/images/places/previews/fb0/298/58-/thumbnail/hifi-place-fb029858-8d3c-4581-83d2-2e254332fc92.jpg?1559231621\",
                    \"lobby\":\"https://hifi-metaverse.s3-us-west-1.amazonaws.com/images/places/previews/fb0/298/58-/lobby/hifi-place-fb029858-8d3c-4581-83d2-2e254332fc92.jpg?1559231621\"
                }
            }
        }}"

PUT https://DOMAINSERVER/api/v1/user/location
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n    
            \"location\": {\n        
                \"availability\": \"connections\",\n        
                \"connected\": true,\n        
                \"domain_id\": \"b7a3ac63-110b-491e-b8e6-5b39a0f613ca\",\n        
                \"network_address\": \"159.203.67.226\",\n        
                \"network_port\": 40102,\n        
                \"node_id\": \"6f153d4a-27db-46ce-80ea-d4fafeff8f87\",\n        
                \"path\": \"/-45.9366,-10.3545,42.2032/0,-0.0897409,0,0.995965\",\n        
                \"place_id\": \"fb029858-8d3c-4581-83d2-2e254332fc92\"\n    
            }\n
        }\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"

GET https://DOMAINSERVER/api/v1/commerce/entity_edition/64e00ba1-0e9a-4052-a360-4fd5651c4b3e.fst?certificate_id=MEYCIQC3TlBcvGAxOHomUmiXxoU8cVnkRXiasSPjy5lOtgCJbwIhAMyf5UVOEfM5H209V%2BDwe%2BvI9MoTAbMLFh9QjnQxgs%2BZ",
    No Authentication header
    Body size zero
    Response: 304 Not Modified

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"
    
GET http://52.85.77.54/6af97d4e-1338-4b4f-97bd-4d80449d5542-v1/clownHammerPhys.mtl
    Header:
        "Host": "mpassets.highfidelity.com"
    Response: 404 Not Found
    Header:
        "Server": "Windows-Azure-Blob/1.0 Microsoft-HTTPAPI/2.0"
    "mimeType": "application/xml",
    "text": "\u00ef\u00bb\u00bf<?xml version=\"1.0\" encoding=\"utf-8\"?><Error><Code>BlobNotFound</Code><Message>The specified blob does not exist.\nRequestId:34c5a207-201e-00e3-1988-bee736000000\nTime:2019-12-29T20:44:53.8307756Z</Message></Error>"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Roughness_COMPRESSED_RED_RGTC1.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=0-999"
    Response: 206 Partial Content
    "content": {
        "size": 1000,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "q0tUWCAxMbsNChoKAQIDBAAAAAABAAAAAAAAALuNAAADGQAAAAIAAAACAAAAAAAAAAAAAAEAAAAKAAAAWAAAAC0AAABoaWZpLmdwdQABAACAPwAAgD8AAIA/AACAPxAAAAALBwAAAAAA/wAAAAABF9cAAAAgAAAAaGlmaS5zb3VyY2VIYXNoANH/a5/UXL3oq3dA8KxTVF4AAAIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==",
        "encoding": "base64" 

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Roughness_COMPRESSED_RED_RGTC1.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=-5516"
    Response: 206 Partial Content
      "content": {
        "size": 5516,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
    "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Albedo_COMPRESSED_SRGB_S3TC_DXT1_EXT.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=0-999"
    Response: 206 Partial Content
      "content": {
        "size": 1000,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Normal_COMPRESSED_RG_RGTC2.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=0-999"
    Response: 206 Partial Content
      "content": {
        "size": 1000,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Normal_COMPRESSED_RG_RGTC2.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=-5516"
    Response: 206 Partial Content
      "content": {
        "size": 5516,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Albedo_COMPRESSED_SRGB_S3TC_DXT1_EXT.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=-5516"
    Response: 206 Partial Content
      "content": {
        "size": 5516,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Metallic_COMPRESSED_RED_RGTC1.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=0-999"
    Response: 206 Partial Content
      "content": {
        "size": 1000,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Metallic_COMPRESSED_RED_RGTC1.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=-5516"
    Response: 206 Partial Content
      "content": {
        "size": 5516,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Metallic_COMPRESSED_RED_RGTC1.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=0-999"
    Response: 206 Partial Content
      "content": {
        "size": 1000,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Metallic_COMPRESSED_RED_RGTC1.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=-5516"
    Response: 206 Partial Content
      "content": {
        "size": 5516,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Normal_COMPRESSED_RG_RGTC2.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=0-999"
    Response: 206 Partial Content
      "content": {
        "size": 1000,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Roughness_COMPRESSED_RED_RGTC1.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=0-999"
    Response: 206 Partial Content
      "content": {
        "size": 1000,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Roughness_COMPRESSED_RED_RGTC1.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=-5516"
    Response: 206 Partial Content
      "content": {
        "size": 5516,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Normal_COMPRESSED_RG_RGTC2.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=-5516"
    Response: 206 Partial Content
      "content": {
        "size": 5516,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Albedo_COMPRESSED_SRGB_S3TC_DXT1_EXT.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=-5516"
    Response: 206 Partial Content
      "content": {
        "size": 5516,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

GET http://52.219.120.130/DomainContent/tanks-corner-baked/content/Pistachio_HiFi/baked/Pistachio_Albedo_COMPRESSED_SRGB_S3TC_DXT1_EXT.ktx
    Header:
        "Host": "hifi-content.s3.amazonaws.com"
        "Range": "bytes=0-999"
    Response: 206 Partial Content
      "content": {
        "size": 1000,
        "compression": 0,
        "mimeType": "image/ktx",
        "text": "TEXT REMOVED FOR BREVITY",
        "encoding": "base64"

PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"


PUT https://DOMAINSERVER/api/v1/user/heartbeat
    Header:
        "Authorization": "Bearer 5ddc1e1981664f628ebaa2a55153d3ecc14e677f549a0c49bbed355b54e11c13"
    "postData": {
        "mimeType": "application/json",
        "text": "{\n}\n",
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\",\"data\":{\"session_id\":\"93fbc10e-626c-4194-9b95-96ce3980aec8\"

POST https://DOMAINSERVER/api/v1/user_activities
    Header:
        no "Authorization"
        "HFM-SessionID": "93fbc10e-626c-4194-9b95-96ce3980aec8"
    "postData": {
      "mimeType": "application/json",
      "text": "{\"action_name\": \"quit\"}",
      "params": []
    }
    Response: 200 OK
        "mimeType": "application/json; charset=utf-8",
        "text": "{\"status\":\"success\"}"

</pre>

[Vircadia]: https://vircadia.com/
[HighFidelity]: https://en.wikipedia.org/wiki/High_Fidelity_(company)
