## Cleaned Up API URL References in Vircadia Interface

As of 20200426.


```

============ grep v1 | grep api ====================

./domain-server/resources/web/wizard/js/wizard.js:
        $.getJSON(URLs.METAVERSE_URL + '/api/v1/domains/' + Settings.data.values.metaverse.id, function(data) {

./domain-server/resources/web/settings/js/settings.js:
        $.post(METAVERSE_URL + '/api/v1/domains/temporary', function(data){

./scripts/tutorials/getDomainMetadata.js:
        var SERVER = Account.metaverseServerURL + '/api/v1';

./scripts/system/commerce/wallet.js:
        var notificationPollEndpointArray = ["/api/v1/commerce/available_updates?per_page=10"];
        notificationPollEndpointArray[1] = "/api/v1/commerce/history?per_page=10";

./scripts/system/makeUserConnection.js:
        var pollCount = 0, requestUrl = Account.metaverseServerURL + '/api/v1/user/connection_request';
        request(Account.metaverseServerURL + '/api/v1/users/' + Account.username + '/location', function (error, response) {

./scripts/system/tablet-goto.js:
        var endpoint = '/api/v1/user_stories?' + options.join('&');

./scripts/system/snapshot.js:
        uri: METAVERSE_BASE + '/api/v1/user_stories/' + message.story_id,
        uri: METAVERSE_BASE + '/api/v1/user_stories',
        uri: METAVERSE_BASE + '/api/v1/user_stories/' + message.story_id,
        var url = METAVERSE_BASE + "/api/v1/user_stories?" + options.join('&');
        uri: METAVERSE_BASE + '/api/v1/user_stories/' + element,

./scripts/system/interstitialPage.js:
        var url = Account.metaverseServerURL + '/api/v1/places/' + domain;

./scripts/system/libraries/connectionUtils.js:
        url = METAVERSE_BASE + '/api/v1/users?per_page=400&'
        url = METAVERSE_BASE + '/api/v1/users?filter=connections'
./scripts/system/html/users.html:
        var METAVERSE_API_URL = "https://metaverse.highfidelity.com/api/v1/users?status=online";

./scripts/system/pal.js:
        uri: METAVERSE_BASE + '/api/v1/user/connections/' + connectionUserName,
        uri: METAVERSE_BASE + '/api/v1/user/friends/' + friendUserName,
        uri: METAVERSE_BASE + '/api/v1/user/friends',
        var url = METAVERSE_BASE + '/api/v1/users?per_page=' + (numResultsPerPage || SAFETY_LIMIT) + '&';
        var url = METAVERSE_BASE + '/api/v1/users?filter=connections&per_page=' + SAFETY_LIMIT + '&search=' + encodeURIComponent(specificUsername);
        notificationPollEndpoint: ["/api/v1/users?filter=connections&status=online&per_page=10"],

./libraries/entities/src/EntityTree.cpp:
        requestURL.setPath("/api/v1/commerce/proof_of_purchase_status/location");
        requestURL.setPath("/api/v1/commerce/proof_of_purchase_status/transfer");
        
./libraries/entities/src/EntityItem.cpp:
        requestURL.setPath("/api/v1/commerce/marketplace_key");

./libraries/networking/src/AddressManager.h:
        const QString GET_PLACE = "/api/v1/places/%1";

./libraries/networking/src/AccountManager.cpp:
        profileURL.setPath("/api/v1/user/profile");
        lockerURL.setPath("/api/v1/user/locker");
        lockerURL.setPath("/api/v1/user/locker");
        const QString USER_PUBLIC_KEY_UPDATE_PATH = "api/v1/user/public_key";
        const QString DOMAIN_PUBLIC_KEY_UPDATE_PATH = "api/v1/domains/%1/public_key";

./libraries/networking/src/AddressManager.cpp:
        const QString GET_DOMAIN_ID = "/api/v1/domains/%1";
        const QString GET_USER_LOCATION = "/api/v1/users/%1/location";

./libraries/networking/src/UserActivityLogger.h:
        const QString USER_ACTIVITY_URL = "/api/v1/user_activities";

./server-console/src/modules/hf-notifications.js:
        const STORIES_URL= '/api/v1/user_stories';
        const USERS_URL= '/api/v1/users';
        const ECONOMIC_ACTIVITY_URL= '/api/v1/commerce/history';
        const UPDATES_URL= '/api/v1/commerce/available_updates';

./launchers/qt/src/SignupRequest.cpp:
        signupURL.setPath("/api/v1/user/channel_user");

./launchers/qt/src/UserSettingsRequest.cpp:
        lockerURL.setPath("/api/v1/user/locker");

./assignment-client/src/avatars/MixerAvatar.cpp:
        static const QString POP_MARKETPLACE_API { "/api/v1/commerce/proof_of_purchase_status/transfer" };

./ice-server/src/IceServer.cpp:
        QString publicKeyPath = QString("/api/v1/domains/%1/public_key").arg(uuidStringWithoutCurlyBraces(domainID));

./android/apps/interface/src/main/java/io/highfidelity/hifiinterface/provider/UserStoryDomainProvider.java:
        @GET("api/v1/user_stories")

./android/apps/interface/src/main/java/io/highfidelity/hifiinterface/provider/EndpointUsersProvider.java:
        @GET("api/v1/users")
        @DELETE("api/v1/user/connections/{connectionUserName}")
        @DELETE("api/v1/user/friends/{friendUserName}")
        @POST("api/v1/user/friends")

./interface/resources/qml/hifi/commerce/checkout/Checkout.qml:
        var MARKETPLACE_API_URL = Account.metaverseServerURL + "/api/v1/marketplace/items/";

./interface/resources/qml/hifi/commerce/common/sendAsset/SendAsset.qml:
        endpoint: "/api/v1/users?filter=connections";

./interface/resources/qml/hifi/AvatarApp.qml:
        var url = "https://highfidelity.com/api/v1/marketplace/items/" + marketId;

./interface/resources/qml/hifi/Pal.qml:
        endpoint: "/api/v1/users/connections";

./interface/resources/qml/hifi/Feed.qml:
        endpoint: '/api/v1/user_stories?' + options.join('&') + (PlatformInfo.isStandalone() ? '&standalone_optimized=true' : '')

./interface/src/commerce/QmlMarketplace.cpp:
        const QString URL = "/api/v1/marketplace/";

./interface/src/commerce/Ledger.cpp:
        const QString URL = "/api/v1/commerce/";

./interface/src/scripting/GooglePolyScriptingInterface.cpp:
        const QString LIST_POLY_URL = "https://poly.googleapis.com/v1/assets?";
        const QString GET_POLY_URL = "https://poly.googleapis.com/v1/assets/model?";

./interface/src/avatar/MarketplaceItemUploader.cpp:
        static const QString path = "/api/v1/marketplace/categories";
        QString path = "/api/v1/marketplace/items";
        static const QString path = "/api/v1/commerce/inventory";

./interface/src/avatar/MyAvatar.cpp:
        // - Sometimes (such as the response from /api/v1/users/:username/location), the location can be stale, but there is a

./interface/src/Constants.h:
        static const QString API_SIGNUP_PATH = "api/v1/users";

./interface/src/DiscoverabilityManager.cpp:
        const QString API_USER_LOCATION_PATH = "/api/v1/user/location";
        const QString API_USER_HEARTBEAT_PATH = "/api/v1/user/heartbeat";

./interface/src/ui/Snapshot.cpp:
        const QString SNAPSHOT_UPLOAD_URL = "/api/v1/snapshots";

./interface/src/ui/overlays/ContextOverlayInterface.cpp:
        requestURL.setPath("/api/v1/commerce/proof_of_purchase_status/transfer");

./interface/src/ui/SnapshotUploader.cpp:
        const QString STORY_UPLOAD_URL = "/api/v1/user_stories";

./interface/src/ui/LoginDialog.cpp:
        const QString LINK_OCULUS_PATH = "api/v1/user/oculus/link";
        const QString CREATE_ACCOUNT_FROM_OCULUS_PATH = "api/v1/user/oculus/create";
        const QString LINK_STEAM_PATH = "api/v1/user/steam/link";
        const QString CREATE_ACCOUNT_FROM_STEAM_PATH = "api/v1/user/steam/create";

./script-archive/avatarSelector.js:
        req.open("GET", "https://metaverse.highfidelity.com/api/v1/marketplace?category=head+%26+body&limit=21", false);

./script-archive/lobby.js:
        req.open("GET", "https://metaverse.highfidelity.com/api/v1/places?limit=21", false);

./domain-server/src/DomainServerSettingsManager.h:
        // these are used to locally cache the result of calling "api/v1/groups/.../is_member/..." on metaverse's api
        // remember the responses to api/v1/groups/%1/ranks

./domain-server/src/DomainServerSettingsManager.cpp:
        const QString GET_GROUP_ID_PATH = "api/v1/groups/names/%1";
        const QString GET_GROUP_RANKS_PATH = "api/v1/groups/%1/ranks";

./domain-server/src/DomainGatekeeper.cpp:
        const QString USER_PUBLIC_KEY_PATH = "api/v1/users/%1/public_key";
        const QString PUBLIC_KEY_URL_REGEX_STRING = "api\\/v1\\/users\\/([A-Za-z0-9_\\.]+)\\/public_key";
        const QString GET_IS_GROUP_MEMBER_PATH = "api/v1/groups/members/%2";
        const QString GROUP_MEMBERSHIPS_URL_REGEX_STRING = "api\\/v1\\/groups\\/members\\/([A-Za-z0-9_\\.]+)";
        const QString GET_FRIENDS_LIST_PATH = "api/v1/user/friends";

./domain-server/src/DomainMetadata.cpp:
        static const QString DOMAIN_UPDATE = "/api/v1/domains/%1";

=====================================
./domain-server/src/DomainServer.cpp:
        accountManager->sendRequest("/api/v1/domains/temporary", AccountManagerAuth::None,
        accountManager->sendRequest("api/v1/transactions",
        static const QString DOMAIN_UPDATE = "/api/v1/domains/%1";
        static const QString DOMAIN_ICE_ADDRESS_UPDATE = "/api/v1/domains/%1/ice_server_address";
        return forwardMetaverseAPIRequest(connection, "/api/v1/domains", "");
        return forwardMetaverseAPIRequest(connection, "/api/v1/domains/" + id, "", {}, {}, false);
        return forwardMetaverseAPIRequest(connection, "/api/v1/user/places", "");
        return forwardMetaverseAPIRequest(connection, "/api/v1/domains", "domain", { "label" });
        return forwardMetaverseAPIRequest(connection, "/api/v1/domains/" + domainID, "domain",
        QUrl url { MetaverseAPI::getCurrentMetaverseServerURL().toString() + "/api/v1/places/" + place_id };
        profileURL.setPath("/api/v1/user/profile");

        
```

