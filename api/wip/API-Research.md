# Vircadia/Project Apollo API Research

The [Vircadia] project needs a server infrastructure.
This project is [Project Apollo].

This article is my progress in reverse engineering the API used by
the Interface to talk to the servers.

The methodology as been to grep all the Interface sources
using ```find . -type f -print0 | xargs -0 grep v1 | grep api```
and then do some cleanup to remove some libraries.
The [cleaned up and organized output is here].

The from the Interface to the services is a REST interface.
Marcus Llewellyn sent me an initial [Swagger]
description of the [service API calls].
The work I'm putting in here is to verify all the API functions
and look for various authentication and data requirements.
My goal is to complete the above beginning and potentially
complete a [Swagger] definition so I can do code generation
for a complete service interface in whatever scalable
service infrastructure is chosen.

This is not the most efficient way of doing this but
it will be good for me to get into the code.
It will be a learning experience.

## Misc notes

When AccountManager makes REST requests, it:

  * sets FollowRedirectsAttribute to 'true'
  * sets QNetworkRequest::UserAgentHeader to HIGH_FIDELITY_USER_AGENT = "Mozilla/5.0 (HighFidelityInterface)"
  * sets METAVERSE_SESSION_ID_HEADER ("HFM-SessionID") to _sessionID (guid)
  * if authentication is required, sets "Authentication:" header to 'accessToken'

Some of the REST requests rely on QNetworkRequest attributes that are returned with
the response. For instance, "ice-server/src/IceServer.cpp" sets the QNetworkRequest::User
attriubute to the 'domainID' and then expects to extract the 'domainID' from the
attributes in the response. The server must implement this by sub-classing QNetworkAccessManager.
Need to understand the format of HTTP attributes in Qt.

## Individual API Request Notes

<pre>

METAVERSE_URL/api/metaverse_info
    domain-server/resources/web/js/shared.js
        GET in function getMetaverseUrl() returns 'metaverse_url'

/api/v1/domains/{metaverse.id}
/api/domains/{metaverse.id}    (?? see note below)
    domain-server/resources/web/wizard/js/wizard.js
        GET expects 'domain.default_place_name' to updatePlaceNameLink(domain.default_place_name)
            or 'domain.name' ("Temporty name") to updatePlaceNameLink(domain.name)
            or 'domain.network_address'
                if 'domain.network_port' != 40102 then
                    updatePlaceNameLink(domain.network_address)
                else updatePlaceNameLink(domain.network_address + ':' + domain.network_port
        (same logic in domain-server/resources/settings/js/settings.js in 'getShareName()')
        (the JavaScript logic is an intertwined mess of app logic and presentation)
    domain-server/resources/settings/js/settings.js
        domain.owner_places
        domain.label
    domain-server/resources/web/js/shared.js
        'getDomainFromAPI' references '/api/domains/{metaverse.id}' to get above data
    libraries/networking/src/AddressManager.cpp
    domain-server/resources/web/js/shared.js
        GET 'domain': {'cloud_domain': boolean }    // returned by isCloudDomain()
    domain-server/src/DomainServer.cpp
        PUT to do a domain update
            {
                'version': BuildInfo::VERSION,
                'protocol': protocolVersionsSignatureBase64(),
                'network_address': networkAddress,
                'automatic_networking': string,
                'restricted': ifAccessRestricted,
                'api_key':  suppliedIfTemporaryDomain
                'heart_beat': numberOfUsers??
            }
    domain-server/src/DomainMetadata.cpp
        PUT
            {
                'domain': JSONstringOfDescriptors
                from comment in this file:
                    { "description": String, // capped description
                      "capacity": Number,
                      "restriction": String, // enum of either open, hifi, or acl
                      "maturity": String, // enum corresponding to ESRB ratings
                      "hosts": [ String ], // capped list of usernames
                      "tags": [ String ], // capped list of tags
                    }
/api/v1/domains/{metaverse.id}/public_key
    ice-server/src/IceServer.cpp
        adds 'FollowRedirectionsAttibute' to 'true'
        adds 'user' with domainID ("publicKeyRequest.setAtribute(QNeweorRequest::User, domainId)")
        GET
            {
                'public_key': fromBase64(dataObject[PUBLIC_KEY_KEY].toString().toUtf8())
            }


METAVERSE_URL/api/v1/domains/temporary
    domain-server/resources/web/js/shared.js
        GET expects 'domain.id'
    domain-server/src/DomainServer.cpp
        expects 'domain.id', 'domain.name', 'domain.api_key'

/api/v1/domains/{id}
    scripts/tutorials/getDomainMetadata.js
        'domain' has a lot of info in it (metadata)

/api/v1/domains/{id}/ice_server_address
    domain-server/src/DomainServer.cpp
        PUT to update ICE server address
            {
                'ice_server_address': '0.0.0.0' or addressAsString,
                'api_key': APIkeyIfTemporaryDomain
            }
            'domain': "aboveInfoAsJSONString"
                (there is an extra toJson() for unknown reasons)

api/v1/transactions
    domain-server/src/DomainServer.cpp
        GET expects 'transaction.id' (UUID), 'transaction.destination_wallet_id',
                    'transaction.amount'
        does one request for each "pending assignment credit"

/api/v1/user/places&limit=21
    script-archive/lobby.js
        GET
            expects JSON to display lobby previews
                data.places[N].previews.lobby to give a picture/texture
            

/api/v1/user/places/{placeID}
/api/v1/user/places/{placeName}
    scripts/tutorials/getDomainMetadata.js
        'place.domain.id'
    scripts/system/interstitialPage.js
        GET
            { 'place': {
                'description': text
                }
            }
    libraries/networking/src/AddressManager.h

/api/v1/user/profile
    libraries/networking/src/AccountManager.cpp
        adds 'Authorization:' header token
        GET
            {
                'user': {
                    'username': name,
                    'xmpp_password': pwString,
                    'discourse_api_key': key,
                    'wallet_id': id

                }
            }

/api/v1/user/profile?access_token=accessToken
    domain-server/src/DomainServer.cpp

/api/v1/user/locker
    libraries/networking/src/AccountManager.cpp
        adds 'Authorization:' header token
        GET 
            {
                'home_location':
            }
    launchers/qt/src/SignupRequest.cpp
        adds "Authorization:" header either "Bearer {token}"
        GET 
            {
                'home_location':
            }


/api/v1/user/public_key
    libraries/networking/src/AccountManager.cpp
        PUT
            header: 'form-data; name="public_key"; filename="public_key"'
            header: 'form-data; name="api_key"'
        Uses this URL or one below depending if 'domainID' is null or not

/api/v1/user/connections/{username}
/api/v1/user/connections
/api/v1/user/friends/{username}
/api/v1/user/friends

/api/v1/users/{username}/public_key
    domain-server/src/DomainGatekeeper.cpp
        GET
            {
                'data': {
                    'public_key': publicKeyInBase64
                },
                'status': 'success'
            }

/api/v1/users?per_page=400&
/api/v1/users?filter=connections
/api/v1/users?filter=connections
                &status=online
                &page=1
                &per_page=MAX_NOTIFICATION_ITEMS
/api/v1/users?search=specificUserName
/api/v1/users?status=online
https://metaverse.highfidelity.com/api/v1/users?status=online
https://metaverse.highfidelity.com/api/v1/users?status=online&filter=friends
    scripts/system/html/users.html
        GET
            { 'users': [
                    {
                        'username': userName,
                        'location': {
                            'root': {
                                'name': locationName
                            }
                        }
                    },
                    ...
                ]
            }
    scripts/system/libraries/connectionUtils.js
        part of 'getAvailableConnections()'
        GET
            { 'users': [
                    {
                        'username': userName,
                        'connection': string,
                        'images': {
                            'thumbnail': url
                        },
                        'location': {
                            'node_id': usedAsSessionId,
                            'root': {
                                'name': locationName
                            },
                            'domain': {     // seems to expect either 'root' or 'domain'
                                'name': locationName
                            }
                        }
                    },
                    ...
                ]
            }
    scripts/system/pal.js
    server-console/src/modules/hf-notifications.js
        adds 'auth', 'bearer': token to JavaScript request.get()
        does polling to generate notifications about new users
        GET
            { 'users': [
                {
                    'username': userName,
                }
            ]
            }
    android/apps/interface/src/main/java/io/highfidelity/hifiinterface/provider/EndpointUsersProvider.java
        GET // function called 'getUsers'
            {
                'status': 'success',
                'current_page':
                'total_pages':
                'per_page':
                'total_entries':
                'users': [
                    {
                        'username': string,
                        'online': boolean,
                        'connection': string,
                        'images': {
                            'hero': string,
                            'thumbnail': string,
                            'tiny': string
                        },
                        'location': {
                            'root': {
                                'name': string
                            },
                            'domain': {
                                'name': string
                            }
                        }
                    }
                ]
            }
    interface/resources/qml/hifi/commerce/common/sendAsset/SendAsset.qml

/api/v1/user/friends
    scripts/system/pal.js
        POST    // to "addFriend"
            {
                'username': friendUserName
            }
    android/apps/interface/src/main/java/io/highfidelity/hifiinterface/provider/EndpointUsersProvider.java
        POST    // part of routine 'addFriend'
            body as above
    domain-server/src/DomainGatekeeper.cpp
        GET
            {
                status: "success",
                data: {
                    friends: [
                        "chris",
                        "freidrica",
                        "G",
                        "huffman",
                        "leo",
                        "philip",
                        "ryan",
                        "sam",
                        "ZappoMan"
                    ]
                }
            }

/api/v1/user/friends/{friendUserName}
    scripts/system/pal.js
        DELETE to "removeFriend"
    android/apps/interface/src/main/java/io/highfidelity/hifiinterface/provider/EndpointUsersProvider.java
        DELETE  // part of routine 'removeFriend'


/api/v1/user_stories/{story_id}
    scripts/system/snapshot.js
        GET
            {
                'user_story': {
                    'path': storyPath,
                    'place_name': storyPlaceName,
                    'thumbnail_url': storyThumbnailUrl,
                    'details': {
                        'sharable_url': storySharableUrl,
                        'image_url': storyImageUrl
                    }
                }
            }

/api/v1/user_stories
    scripts/system/snapshot.js
        POST
            { 'user_story': {
                'audience: 'for_connections',   // part of "BlastToConnections"
                'action': 'announcement',
                'path': storyPath,
                'place_name': storyPlaceName,
                'thumbnail_url': storyThumbnailUrl
                // There is comment that "for historical reasons" the server doesn't
                //    take nested JSON objects so the 'details' contents are stringified
                'details': {
                    'sharable_url': storySharableUrl,
                    'image_url': storyImageUrl
                    }
                }
            }
    interface/src/ui/SnapshotUploader.cpp
        POST    // for uploading a snapshot
            {
                'user_story': {
                    'details': {    // contents turned to JSON so single level JSON struct
                        'image_url': url,
                        'sharable_url': shareableURL // if defined
                        'snapshot_id': snapshotId,
                        'original_image_file_name': filename
                    }
                    'thumbnail_url':
                    'place_name':
                    'path':
                    'action': 'snapshot',   // for uploading snapshot
                    'audience': 'for_url'
                }
            }

/api/v1/user_stories?include_actions=concurrency    // actions for places
                    &include_actions=concurrency,announcements,snapshot    // actions for full search
                    &restriction=open   // if logged in, "restriction=open,hifi"
                    &require_online=true
                    &protocol=??        // "encodeURIComponent(Window.protocolSignature())"
                    &tags=??
                    &page=pageNumber
                    &standalone_optimized=true  // if the platform is standalone
    android/apps/interface/src/main/java/io/highfidelity/hifiinterface/provider/UserStoryDomainProvider.java
    interface/resources/qml/hifi/Feed.qml


/api/v1/user_stories?include_actions=announcement
                    &restriction=open,hifi
                &require_online=true&protocol={protocolSignature}&per_page=10
    scripts/system/tablet-goto.js
        GET

/api/v1/user_stories?since=sinceSeconds
                    &include_actions=announcement
                    &restriction=open,hifi
                    &require_online=true
                    &per_page=MAX_NOTIFICATION_ITEMS
    server-console/src/modules/hf-notifications.js
        GET
            does above GET then immediately does another
            /api/v1/user_stories?now="new Date().toISOString()"
                                &include_actions=announcement
                                &restriction=open,hifi
                                &require_online=true
                                &per_page=MAX_NOTIFICATION_ITEMS
            adds 'auth'=token to JavaScript request()
            { 'user_stories': [
                { 'place_name': storyPlaceName,
                    ...
                }
            ]
            }
            


/api/v1/user_activities
    libraries/networking/src/UserActivityLogger.h
        POST
            'form-data; name="action_name"' actionString
            'form-data; name="elapsed_ms"'  _timer.elapsed()
            'form-data; name="action_details"' jsonString
        actions:
            launch
                version
                previousSessionCrashed
                previousSessinRuntime
            insufficient_gl
                glData
            changed_display_name
                display_name
            changed_model
                type_of_model
                model_url
            changed_domain
                domain_url
            connected_devices
                type_of_device
                device_name
            landed_script
                script_name
            went_to
                trigger
                    UserINput, Back, Forward, StartupFromSettings, Suggestions
                destination_type
                destination_name

/api/v1/snapshots
    interface/src/ui/Snapshot.cpp
        POST    // to upload snapshot
            Header 'ContentType:' set to type of image (gif or jpeg)
            'form-data;name=image;filename=theFilename'

/api/v1/groups/.../is_member/...
    domain-server/src/DomainServerSettingsManager.h
        from comment in above file
    
/api/v1/groups/members/{userName}
    domain-server/src/DomainGatekeeper.cpp
        GET
            {
                "data":{
                    "username":"sethalves",
                    "groups":{
                        "fd55479a-265d-4990-854e-3d04214ad1b0":{
                            "name":"Blerg Blah",
                            "rank":{
                                "name":"admin",
                                "order":1
                            }
                        }
                    }
                },
                "status":"success"
            }
`
/api/v1/groups/names/{groupName}
    domain-server/src/DomainServerSettingsManager.cpp
        GET
            {   // from comment in the source code
                "data":{
                    "groups":[{
                        "description":null,
                        "id":"fd55479a-265d-4990-854e-3d04214ad1b0",
                        "is_list":false,
                        "membership":{
                            "permissions":{
                                "custom_1=":false,
                                "custom_2=":false,
                                "custom_3=":false,
                                "custom_4=":false,
                                "del_group=":true,
                                "invite_member=":true,
                                "kick_member=":true,
                                "list_members=":true,
                                "mv_group=":true,
                                "query_members=":true,
                                "rank_member=":true
                            },
                            "rank":{
                                "name=":"owner",
                                "order=":0
                            }
                        },
                        "name":"Blerg Blah"
                    }]
                },
                "status":"success"
            }

/api/v1/groups/{groupID}/ranks
    domain-server/src/DomainServerSettingsManager.cpp
        GET

            {
                "data":{
                    "groups":{
                        "d3500f49-0655-4b1b-9846-ff8dd1b03351":{
                            "members_count":1,
                            "ranks":[
                                {
                                    "id":"7979b774-e7f8-436c-9df1-912f1019f32f",
                                    "members_count":1,
                                    "name":"owner",
                                    "order":0,
                                    "permissions":{
                                        "custom_1":false,
                                        "custom_2":false,
                                        "custom_3":false,
                                        "custom_4":false,
                                        "edit_group":true,
                                        "edit_member":true,
                                        "edit_rank":true,
                                        "list_members":true,
                                        "list_permissions":true,
                                        "list_ranks":true,
                                        "query_member":true
                                    }
                                }
                            ]
                        }
                    }
                },"status":"success"
            }

domain-server/src/DomainServer.cpp
    passes through
        /api/v1/domains
        /api/v1/domains/{domainID}
        /api/v1/user/places


/api/v1/marketplace/
    ./interface/src/commerce/QmlMarketplace.cpp:
/api/v1/marketplace?category=head+%26+body&limit=21", false);
    ./script-archive/avatarSelector.js:
/api/v1/marketplace/items/
    ./interface/resources/qml/hifi/commerce/checkout/Checkout.qml:
/api/v1/marketplace/items/{marketId}
    ./interface/resources/qml/hifi/AvatarApp.qml:
/api/v1/marketplace/categories
    ./interface/src/avatar/MarketplaceItemUploader.cpp:
/api/v1/marketplace/items
    ./interface/src/avatar/MarketplaceItemUploader.cpp:
/api/v1/commerce/inventory
    ./interface/src/avatar/MarketplaceItemUploader.cpp:

/api/v1/commerce/
    ./interface/src/commerce/Ledger.cpp:
/api/v1/commerce/inventory
    ./interface/src/avatar/MarketplaceItemUploader.cpp:
/api/v1/commerce/available_updates?per_page=10
    ./scripts/system/commerce/wallet.js:
/api/v1/commerce/history?per_page=10
    ./scripts/system/commerce/wallet.js:
/api/v1/commerce/marketplace_key
    ./libraries/entities/src/EntityItem.cpp:
/api/v1/commerce/proof_of_purchase_status/location
    ./libraries/entities/src/EntityTree.cpp:
/api/v1/commerce/proof_of_purchase_status/transfer
    ./libraries/entities/src/EntityTree.cpp:
    ./assignment-client/src/avatars/MixerAvatar.cpp:
    ./interface/src/ui/overlays/ContextOverlayInterface.cpp:
/api/v1/commerce/history
    ./server-console/src/modules/hf-notifications.js:
/api/v1/commerce/available_updates
    ./server-console/src/modules/hf-notifications.js:


https://poly.googleapis.com/v1/assets?
https://poly.googleapis.com/v1/assets/model?
    interface/src/scripting/GooglePolyScriptingInterface.cpp
</pre>

## Legal stuff

The contents if this page are licensed under the 
[Creative Commons Attribution-NonCommercial 4.0 International]
license.

[Vircadia]: https://vircadia.com/
[Project Apollo]: https://github.com/kasenvr/project-apollo
[cleaned up and organized output is here]: ./CleanedUpURLs.html
[Swagger]: https://swagger.io/
[service API calls]: https://hifimetaverseapi.azurewebsites.net/index.html
[Apache License]: http://opensource.org/licenses/Apache-2.0
[Creative Commons Attribution-NonCommercial 4.0 International]: http://creativecommons.org/licenses/by-nc/4.0/

<!-- vim: ts=2 sw=2 et ai
-->
