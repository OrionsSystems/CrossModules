/**
 * OrionsCloud JavaScript SDK
 * © 2016 by OrionsWave LLC
   The SDK library contains:
        Orions.Cloud.Client.Settings
        Orions.Cloud.Client.UserProxy
        Orions.Cloud.Client.DataProxy
        Orions.Cloud.Client.InstallationProxy
        Orions.Cloud.Client.SessionProxy
        Orions.Cloud.Client.ContentProxy
        Orions.Cloud.Client.AppVersionSettingsProxy
        Orions.Cloud.Client.LogProxy
        Orions.Cloud.Client.TokenProxy
		Orions.Cloud.Client.PurchaseProxy
        Orions.Cloud.Client.TriggerProxy
        Orions.Cloud.Client.BusMessageProxy
        Orions.Cloud.Common.FilterBulder
        Orions.Cloud.Common.OrderBuilder
        Orions.Cloud.Common.UpdateBuilder
        Orions.Cloud.Common.ProjectionBuilder
 */

var Orions = (function (){

    "use strict";

    var Cloud = (function () {

        var _cloudUrl = 'https://api.orionscloud.com',
            _accessToken = '',
            _origin = '',
            _cloudVersion = '';

        var _appName = '',
            _appKey = '',
            _appVersion = '',
            _OSName = '',
            _OSVersion = '',
            _deviceName = '',
            _deviceModel = '',
            _sdkInfo = 'JavaScript',
            _sdkVersion = '2.16',
            _componentName = '',
            _componentVersion = '',
            _verbose = false;

        var settings = {
            cloudUrl: {
                get:function(){
                    return _cloudUrl;
                },
                set:function(value){
                    _cloudUrl = value;
                }
            },
            cloudVersion: _cloudVersion,
            appName: _appName,
            appVersion: _appVersion,
            OSName: _OSName,
            OSVersion: _OSVersion,
            deviceName: _deviceName,
            deviceModel: _deviceModel,
            componentName:{
                get:function(){
                    return _componentName;
                },
                set:function(value){
                    _componentName = value;
                }
            },
            componentVersion: {
                get:function(){
                    return _componentVersion;
                },
                set:function(value){
                    _componentVersion = value;
                }
            },
            appKey: _appKey,
            verbose: _verbose,
            accessToken: {
                get: function () {
                    return _accessToken;
                },
                set: function (token) {
                    _accessToken = token;
                }
            },
            origin: {
                get: function () {
                    return _origin;
                },
                set: function (value) {
                    _origin = value;
                }
            }
        };

        var updateOptions = (function () {

            "use strict";

            function _addSetUpDefinition(field, condition, value) {
                var _localUpdateData = {};
                if (!_localUpdateData[field] || typeof _localUpdateData[field] === 'string') {
                    _localUpdateData[field] = {};
                }
                _localUpdateData[field][condition] = value;
                return _localUpdateData;
            }

            function _createSimpleDictionary(operation, value) {
                var _localFilterData = {};
                _localFilterData[operation] = value;
                return _localFilterData;
            }

            function _combinedUpdateQueries(updates) {
                var _localUpdateData = {};
                for (var update in updates) {
                    var object = updates[update];
                    for (var element in object) {
                        _localUpdateData[element] = object[element];
                    }
                }
                return _localUpdateData;
            }

            function _pullFilterDefinition(field, filter) {
                var filterDef = _createSimpleDictionary('^filter', filter);
                return _createSimpleDictionary('^pull', filterDef);
            }

            function _pushUpdateDefinition(field, values, slice, position, sort) {
                var params = _createSimpleDictionary('^each', values);
                if (slice) params['^slice'] = slice;
                if (position) params['^position'] = position;
                if (sort) params['^sort'] = sort;
                return _createSimpleDictionary('^push', params);
            }

            function _render(updateBuilder) {
                if (updateBuilder instanceof Object) {
                    return JSON.stringify(updateBuilder);
                } else {
                    return updateBuilder;
                }
            }

            var updateBuilder = {

                /**
                 * Creates an add to set operator.
                 * @param field
                 * @param value
                 * @returns An add to set operator.
                 */
                AddToSet: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    var operation = (value instanceof Array) ? '^each' : '^addToSet';
                    return _addSetUpDefinition(field, operation, value);
                },

                /**
                 * Creates an add to set operator.
                 * The operator adds a value to an array unless the value is already present, in which case does nothing to that array.
                 * @param field
                 * @param value
                 * @returns An add to set operator.
                 */
                AddToSetEach: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    var operation = (value instanceof Array) ? '^each' : '^addToSet';
                    return _addSetUpDefinition(field, operation, value);
                },

                /**
                 * Creates a combined update.
                 * @param updates The updates.
                 * @returns A combined update.
                 */
                Combine: function (updates) {
                    if (!updates) {
                        throw new Error('Updates property cannot be null');
                    }
                    if (!(updates instanceof Array)) {
                        throw new Error('Updates property must be array');
                    }
                    return _combinedUpdateQueries(updates);
                },

                /**
                 * Creates an increment operator.
                 * @param field
                 * @param value
                 * @returns An increment operator.
                 */
                Inc: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSetUpDefinition(field, '^inc', value);
                },

                /**
                 * Creates a pop first operator.
                 * The PopFirst operator removes the first element of an array.
                 * @param field
                 * @returns A pop operator.
                 */
                PopFirst: function (field) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSetUpDefinition(field, '^pop', -1);
                },

                /**
                 * Creates a pop last operator.
                 * The PopLast operator removes the last element of an array.
                 * @param field
                 * @returns A pop operator.
                 */
                PopLast: function (field) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSetUpDefinition(field, '^pop', 1);
                },

                /**
                 * Creates a pull operator.
                 * The pull operator removes from an existing array all instances of a value or values that match a specified condition.
                 * @param field
                 * @param value
                 * @returns A pull operator
                 */
                Pull: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    var operation = (value instanceof Array) ? '^pullAll' : '^pull';
                    return _addSetUpDefinition(field, operation, value);
                },

                /**
                 *Creates a pull operator.
                 * The PullAll operator removes all instances of the specified values from an existing array.
                 * @param field
                 * @param values
                 * @returns A pull operator
                 */
                PullAll: function (field, values) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSetUpDefinition(field, '^pullAll', values);
                },

                /**
                 * Creates a pull operator.
                 * The PullFilter operator removes all instances of the specified values from an existing array that the condition is true
                 * @param filed
                 * @param filter
                 * @returns A pull operator.
                 */
                PullFilter: function (filed, filter) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _pullFilterDefinition(filed, filter);
                },

                /**
                 * Creates a push operator.
                 * The push operator appends a specified value to an array.
                 * @param field
                 * @param value
                 * @returns A push operator.
                 */
                Push: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSetUpDefinition(field, '^push', value);
                },

                /**
                 * Creates a push operator.
                 * Use push with the each modifier to append multiple values to the array field.
                 * @param field
                 * @param values
                 * @param slice
                 * @param position
                 * @param sort
                 * @returns A push operator.
                 */
                PushEach: function (field, values, slice, position, sort) {
                    if (!field || !values) {
                        throw new Error('PushEach parameter can not be empty or null');
                    }
                    return _pushUpdateDefinition(field, values, slice, position, sort);
                },

                /**
                 * Creates a set operator.
                 * @param field
                 * @param value
                 * @returns A set operator.
                 */
                Set: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSetUpDefinition(field, '^set', value);
                }
            };

            return {
                updateBuilder: updateBuilder,
                render: _render
            }
        }());

        var orderOptions = (function () {

            "use strict";

            function _directionalSortOrder(operation, value) {
                var _localFilterData = {};
                _localFilterData[value] = operation;
                return _localFilterData;
            }

            function _combinedSortOrder(sorts) {
                var _localFilterData = {};
                for (var sort in sorts) {
                    var object = sorts[sort];
                    for (var element in object) {
                        _localFilterData[element] = object[element];
                    }
                }
                return _localFilterData;
            }

            var _render = function (orders) {
                if (orders instanceof Object) {
                    return JSON.stringify(orders);
                } else {
                    return orders;
                }
            };

            var SortDirection = {
                Ascending: 1,
                Descending: -1
            };

            var orderBuilder = {

                /**
                 * Creates an ascending sort.
                 * @param value The field.
                 * @returns An ascending sort.
                 */
                Ascending: function (value) {
                    if (!value) {
                        throw new Error('Input parameter can not be empty or null');
                    }
                    return _directionalSortOrder(SortDirection.Ascending, value);
                },

                /**
                 * Creates a combined sort.
                 * @param sorts The sorts.
                 * @returns A combined sort.
                 */
                Combine: function (sorts) {
                    if (!sorts) {
                        throw new Error('Sorts parameter can not be empty or null');
                    }
                    if (!(sorts instanceof Array)) {
                        throw new Error('Sorts parameter must be array');
                    }
                    return _combinedSortOrder(sorts)
                },

                /**
                 * Creates a descending sort.
                 * @param value The field.
                 * @returns A descending sort.
                 */
                Descending: function (value) {
                    if (!value) {
                        throw new Error('Input parameter can not be empty or null');
                    }
                    return _directionalSortOrder(SortDirection.Descending, value);
                },

                /**
                 * Creates a empty definition.
                 */
                Empty: function () {
                    return {};
                }
            };

            return {
                orderBuilder: orderBuilder,
                render: _render
            };

        }());

        var projectionOptions = (function () {

            "use strict";

            function _buildProjection(fields, value) {
                var _localFilterData = {};
                _localFilterData[fields] = value;
                return _localFilterData;
            }

            function _combinedProjections(projections) {
                var _localProjectionData = {};
                var compareTemp = -1;
                for (var projection in projections) {
                    var object = projections[projection];
                    for (var element in object) {
                        if(compareTemp == -1){
                            compareTemp = object[element];
                        }else{
                            if(compareTemp !== object[element] ){
                                throw new Error('Projections must be of some type');
                            }
                        }
                        _localProjectionData[element] = object[element];
                    }
                }
                return _localProjectionData;
            }

            var _render = function (projections) {
                if (projections instanceof Object) {
                    return JSON.stringify(projections);
                } else {
                    return projections;
                }
            };

            var projectionBuilder = {

                Include: function (field) {
                    if (!field) {
                        throw new Error('Input parameter can not be empty or null');
                    }
                    if (field instanceof Array) {
                        throw new Error('Input parameter can not be array');
                    }
                    return _buildProjection(field, 1);
                },

                Exclude: function (field) {
                    if (!field) {
                        throw new Error('Input parameter can not be empty or null');
                    }
                    if (field instanceof Array) {
                        throw new Error('Input parameter can not be array');
                    }
                    return _buildProjection(field, 0);
                },

                Combine: function (projections) {
                    if (!projections) {
                        throw new Error('Combine parameter can not be empty or null');
                    }
                    if (!(projections instanceof Array)) {
                        throw new Error('Combine parameter must be array');
                    }
                    return _combinedProjections(projections);
                }
            };

            return {
                projectionBuilder: projectionBuilder,
                render: _render
            }

        }());

        var filterOptions = (function () {

            function _addSimpleCondition(field, condition, value) {
                var _localFilterData = {};
                if (!_localFilterData[field] || typeof _localFilterData[field] === 'string') {
                    _localFilterData[field] = {};
                }
                _localFilterData[field][condition] = value;
                return _localFilterData;
            }

            function _addCombinedCondition(operation, filters) {
                var _localFilterData = {};
                _localFilterData[operation] = filters;
                return _localFilterData;
            }

            function _addNotCondition(filters) {
                var _localFilterData = {};
                _localFilterData['^not'] = filters;
                return _localFilterData;
            }

            var _render = function (filters) {
                if (filters instanceof Object) {
                    return JSON.stringify(filters);
                } else {
                    return filters;
                }
            };

            var filterBuilder = {

                Render: function(filter){
                    return _render(filter);
                },

                /**
                 * Creates an and filter.
                 * @param filters
                 * @returns A filter.
                 */
                And: function (filters) {
                    if (!filters) {
                        throw new Error('Filters can not be empty or null');
                    }
                    if (!(filters instanceof Array)) {
                        throw new Error('Filters can be array');
                    }
                    return _addCombinedCondition('^and', filters);
                },

                /**
                 * Creates an or filter.
                 * @param filters The filters.
                 * @returns An or filter.
                 */
                Or: function (filters) {
                    if (!filters) {
                        throw new Error('Filters can not be empty or null');
                    }
                    if (!(filters instanceof Array)) {
                        throw new Error('Filters can be array');
                    }
                    return _addCombinedCondition('^or', filters);
                },

                /**
                 * Gets an empty filter. An empty filter matches everything.
                 */
                Empty: function () {
                    return {};
                },

                /**
                 * Creates an equality filter.
                 * @param field The field.
                 * @param value The value.
                 * @returns An equality filter.
                 */
                Eq: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSimpleCondition(field, '^eq', value);
                },

                /**
                 * Creates an exists filter.
                 * @param field The field.
                 * @param values  true or false
                 * @returns An exists filter.
                 */
                Exists: function (field, values) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSimpleCondition(field, '^exists', values);
                },

                /**
                 * Creates a greater than filter.
                 * @param field The field.
                 * @param value The value.
                 * @returns A greater than filter.
                 */
                Gt: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSimpleCondition(field, '^gt', value);
                },

                /**
                 * Creates a greater than or equal filter.
                 * @param field The field.
                 * @param value The value.
                 * @returns  A greater than or equal filter.
                 */
                Gte: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSimpleCondition(field, '^gte', value);
                },

                /**
                 * Creates an in filter.
                 * @param field The field.
                 * @param value The values.
                 * @returns An in filter.
                 */
                In: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    if (!(value instanceof Array)) {
                        throw new TypeError("Value for In paramater must be array");
                    }
                    return _addSimpleCondition(field, '^in', value);
                },

                /**
                 * Creates a less than filter.
                 * @param field The field.
                 * @param value The value.
                 * @returns A less than filter.
                 * @constructor
                 */
                Lt: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSimpleCondition(field, '^lt', value);
                },

                /**
                 * Creates a less than or equal filter.
                 * @param field The field.
                 * @param value The value.
                 * @returns A less than or equal filter.
                 */
                Lte: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSimpleCondition(field, '^lte', value);
                },

                /**
                 * Creates a not equal filter.
                 * @param field The field.
                 * @param value The value.
                 * @returns A not equal filter.
                 */
                Ne: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSimpleCondition(field, '^ne', value);
                },

                /**
                 * Creates a not in filter.
                 * @param field The field.
                 * @param value The values.
                 * @returns A not in filter.
                 */
                Nin: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    if (!(value instanceof Array)) {
                        throw new TypeError("Value for In paramater must be array");
                    }
                    return _addSimpleCondition(field, '^nin', value);
                },

                /**
                 * Creates a not filter.
                 * @param filters The filters.
                 * @returns A not filter.
                 */
                Not: function (filters) {
                    if (!(filters instanceof Array)) {
                        throw new TypeError("Filters property must be array in Not function");
                    }
                    return _addNotCondition(filters);
                },

                /**
                 * Creates a regular expression filter.
                 * @param field The field.
                 * @param value The regex.
                 * @returns A regular expression filter.
                 */
                Regex: function (field, value) {
                    if (!field) {
                        throw new Error('Field property cannot be null');
                    }
                    return _addSimpleCondition(field, '^regex', value);
                },

                /**
                 * Creates a text filter.
                 * @param value The search.
                 * @returns A text filter.
                 */
                Text: function (value) {
                    return _addSimpleCondition('^text', '^search', value);
                }
            };

            return {
                filterBuilder: filterBuilder,
                render: _render
            };

        }());

        var jsonRequester = (function () {

            "use strict";

            var dataAgentKey = 'OrionsCloudDataAgent';
            var orionsCloudAppKey = "OrionsCloudAppKey";
            var orionsCloudHint = 'OrionsCloudHint';

            function send(method, url, data, headerToken, hint) {

                data = data || undefined;
                headerToken = headerToken || {};

                var dfd = $.Deferred();

                $.ajax({
                    url: url,
                    type: method,
                    method: method,
                    contentType: 'application/json',
                    dataType: "json",
                    data: JSON.stringify(data),
                    cache: false,
                    origin: _origin,
                    crossDomain: true,
                    beforeSend: function (xhr, arg2) {

                        if(settings.verbose){
                            console.log('Starting:', url);
                        }

                        xhr.setRequestHeader("Authorization", "Bearer " + headerToken);
                        var dataAgentValue = encodeDataAgent();
                        if (isNotEmpty(dataAgentValue)) {
                            xhr.setRequestHeader(dataAgentKey, dataAgentValue);
                        }
                        if(isNotEmpty(settings.appKey)){
                            xhr.setRequestHeader(orionsCloudAppKey, settings.appKey);
                        }
                        if(isNotEmpty(hint)){
                            xhr.setRequestHeader(orionsCloudHint, JSON.stringify(hint));
                        }
                        xhr.setRequestHeader("Accept", "application/json");
                        return true;
                    },
                    success: function (res) {
                        if(settings.verbose){
                            console.log('Status:', res);
                        }
                        dfd.resolve(res);
                    },
                    error: function (err) {
                        dfd.reject(err);
                    }
                });

                return dfd;
            }

            function getAuth(headerToken) {
                headerToken = headerToken || {};
                return "Bearer " + headerToken;
            }

            function encodeDataAgent() {
                var res = [];

                res.push(productHeaderValue('App', manageAgentData(settings.appName), manageAgentData(settings.appVersion)));
                res.push(productHeaderValue('OS', manageAgentData(settings.OSName), manageAgentData(settings.OSVersion)));
                res.push(productHeaderValue('Device', manageAgentData(settings.deviceName), manageAgentData(settings.deviceModel)));
                res.push(productHeaderValue('SDK', _sdkInfo, _sdkVersion));

                return res.join(' ');
            }

            function manageAgentData(data){
                if(isNotEmpty(data)){
                    return data;
                }
                return '$';
            }

            function productHeaderValue(namespace, name, value) {
                return namespace + ':' + name.replace(/\ /g, '_') + '/' + value.replace(/\ /g, '_');
            }

            function isNotEmpty(value) {
                return (value != null && !(value === ''));
            }

            function get(url, data, headerToken, hint) {
                return send('GET', url, data, headerToken, hint);
            }

            function getByteArray(url, headerToken, success, error, hint) {

                var dfd = $.Deferred();

                var xmlHttpRequest = new XMLHttpRequest();
                xmlHttpRequest.open('GET', url, true);
                xmlHttpRequest.responseType = "arraybuffer";
                xmlHttpRequest.setRequestHeader("Authorization", getAuth(headerToken));
                var dataAgentValue = encodeDataAgent();
                if (isNotEmpty(dataAgentValue)) {
                    xmlHttpRequest.setRequestHeader(dataAgentKey, dataAgentValue);
                }
                if(isNotEmpty(settings.appKey)){
                    xmlHttpRequest.setRequestHeader(orionsCloudAppKey, settings.appKey);
                }
                if(isNotEmpty(hint)){
                    xmlHttpRequest.setRequestHeader(orionsCloudHint, JSON.stringify(hint));
                }

                xmlHttpRequest.onload = function (event) {
                    if (success !== null) {
                        var response = xmlHttpRequest.response;
                        if (response !== null) {
                            var data = new Uint8Array(response);
                            var byteArray = [];
                            for (var i = 0; i < data.length; i++) {
                                byteArray[i] = data[i];
                            }
                            success(byteArray, xmlHttpRequest.status.toString(), null);
                        }
                        else {
                            success(null, xmlHttpRequest.status.toString(), null);
                        }
                    }
                    dfd.resolve();
                };

                xmlHttpRequest.onerror = function (event) {
                    if (error !== null) {
                        dfd.error(null, xmlHttpRequest.status.toString(), xmlHttpRequest.statusText);
                    }
                    dfd.error();
                };

                xmlHttpRequest.send();

                return dfd;
            }

            function post(url, data, headerToken, hint) {
                return send('POST', url, data, headerToken, hint);
            }

            function put(url, data, headerToken, hint) {
                return send('PUT', url, data, headerToken, hint);
            }

            function putByteArray(url, data, headerToken, hint) {

                data = data || undefined;

                var dfd = $.Deferred();

                $.ajax({
                    url: url,
                    type: 'PUT',
                    method: 'PUT',
                    contentType: 'application/octet-stream',
                    //dataType: 'binary',
                    processData: false,
                    data: data,
                    cache: false,
                    //origin: _origin,
                    crossDomain: true,
                    beforeSend: function (xhr, arg2) {
                        xhr.setRequestHeader("Authorization", getAuth(headerToken));
                        var dataAgentValue = encodeDataAgent();
                        if (isNotEmpty(dataAgentValue)) {
                            xhr.setRequestHeader(dataAgentKey, dataAgentValue);
                        }
                        if(isNotEmpty(settings.appKey)){
                            xhr.setRequestHeader(orionsCloudAppKey, settings.appKey);
                        }
                        if(isNotEmpty(hint)){
                            xhr.setRequestHeader(orionsCloudHint, JSON.stringify(hint));
                        }
                        xhr.setRequestHeader("Accept", "application/octet-stream");
                        return true;
                    },
                    success: function (res) {
                        dfd.resolve(res);
                    },
                    error: function (err) {
                        dfd.reject(err);
                    }
                });

                return dfd;
            }

            function del(url, data, headerToken, hint) {
                return send('DELETE', url, data, headerToken, hint);
            }

            return {
                get: get,
                getByteArray: getByteArray,
                post: post,
                put: put,
                putByteArray: putByteArray,
                delete: del
            };

        }());

        var util = {
            encodeQuery: function (data) {
                var ret = [];
                for (var d in data) {
                    var innerData = data[d];
                    if (typeof data[d] === 'object') {
                        innerData = JSON.stringify(data[d]);
                    }
                    ret.push(encodeURIComponent(d) + "=" + encodeURIComponent(innerData));
                }
                return '?' + ret.join("&");
            }
        };

        var userProxy = {

            /**
             * Sign up user
             * Users have the same features as other objects, such as the flexible schema.
             * The differences are that user objects must have an email and password, the password is automatically encrypted and stored securely,
             * and OrionsCloud enforces the uniqueness of the email field.
             * For non 3rd party identity provider sign ups a password is required.
             *
             * If the call is successful, the API will return a user access token that developers can use to to run operations on beahlf of the user.
             * The user represented by the access token will be referred to as the “context user”.
             * To run behalf operations on user's behalf you need to set the authorization header to "Bearer {user-access-token}"
             *
             * @param object
             * @param hint
             * @returns {*}
             */
            signUp: function (object, hint) {
                if(!object) throw Error('SignUp paramaters is null or empty');
                if(!object.user.EmailStatus) object.user.EmailStatus = 0;
                return jsonRequester.post(_cloudUrl + '/user/me', object, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Log in user with email and password
             * @param email
             * @param password
             * @param hint
             * @returns {*}
             */
            logIn: function (email, password, hint) {
                var data = {
                    "grant_type": "password",
                    "username": email,
                    "password": password
                };
                return jsonRequester.post(_cloudUrl + '/oauth', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Log in user with username and password
             * @param username
             * @param password
             * @param hint
             * @returns {*}
             */
            logInWithUsername: function (username, password, hint) {
                var data = {
                    "grant_type": "username",
                    "username": username,
                    "password": password
                };
                return jsonRequester.post(_cloudUrl + '/oauth', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Log in user with token
             * @param token
             * @param hint
             * @returns {*}
             */
            logInWithToken: function (token, hint) {
                var data = {
                    "grant_type": "token",
                    "token": token
                };
                return jsonRequester.post(_cloudUrl + '/oauth', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Log in with Facebook account
             * @param accessToken
             * @param hint
             * @returns {*}
             */
            logInWithFacebook: function (accessToken, hint) {
                var data = {
                    "grant_type": "facebook",
                    "token": accessToken
                };
                return jsonRequester.post(_cloudUrl + '/oauth', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Log In with twitter account
             * @param accessToken
             * @param secret
             * @param hint
             * @returns {*}
             */
            logInWithTwitter: function (accessToken, secret, hint) {
                var data = {
                    "grant_type": "twitter",
                    "token": accessToken,
                    "secret": secret
                };
                return jsonRequester.post(_cloudUrl + '/oauth', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Log out user
             * @returns {*}
             */
            logOut: function (hint) {
                return jsonRequester.delete(_cloudUrl + '/oauth', null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Update context user
             * Nobody except the user is allowed to modify their own data. To change the data on a user that already exists, send a PUT request to the user URL.
             *
             * @param object
             * @param hint
             * @returns {*}
             */
            update: function (object, hint) {
                return jsonRequester.put(_cloudUrl + '/user/me', object, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Partial Update
             * Note: To figure out how to define compound update definitions contact support@orionscloud.com.
             * @param userId
             * @param update
             * @param hint
             * @returns
             */
            updateByIdPartial: function (userId, update, hint) {
                return jsonRequester.put(_cloudUrl + '/user/'+ userId +'/partial', update, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Retrieve context user
             * Once you have a valid session token, you can send a GET request to the user endpoint to retrieve the context user associated with that session token.
             * @returns {*}
             * @param hint
             */
            get: function (hint) {
                return jsonRequester.get(_cloudUrl + '/user/me', null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },
            getById: function (id, hint) {
                return jsonRequester.get(_cloudUrl + '/user/' + id, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Reset user password
             * The call will initiate a reset password request. The user will receive an email with a password reset link.
             * Note: The password reset feature must be enabled through the OrionsCloud portal for this to work.
             * @param email
             * @param hint
             * @returns {*}
             */
            resetPassword: function (email, hint) {
                var data = {
                    'email': email
                };
                return jsonRequester.post(_cloudUrl + '/user/me/password', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            sendConfirmRegistrationEmail: function (email, hint) {
                var data = {
                    'email': email
                };
                return jsonRequester.post(_cloudUrl + '/user/me/sendemailconfirmation', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Change user password
             * The call accepts as parameters the user’s new and current passwords. The current password is required for non 3rd party identity provider sign ups.
             * @param currentPassword
             * @param newPassword
             * @param hint
             * @returns {*}
             */
            changePassword: function (currentPassword, newPassword, hint) {
                var data = {
                    'current': currentPassword,
                    'new': newPassword
                };
                return jsonRequester.put(_cloudUrl + '/user/me/password', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            search: function (filter, pageNumber, pageSize, sortOrder, hint) {
                var data = {
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'filter': filterOptions.render(filter),
                    'sortOrder': orderOptions.render(sortOrder)
                };
                var queryParams = util.encodeQuery(data);
                return jsonRequester.get(_cloudUrl + '/user' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Connect Context User to Installation
             * @param installationId
             * @param hint
             * @returns {*}
             */
            connectInstallation: function (installationId, hint) {
                var data = {
                    'installationId': installationId
                };
                return jsonRequester.put(_cloudUrl + '/user/me/installation', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Get User Installations
             * @param userId
             * @param hint
             * @returns {*}
             */
            getInstallations: function (userId, hint) {
                if (!userId) {
                    throw new Error('UserId can not be empty or null');
                }
                return jsonRequester.put(_cloudUrl + '/user/' + userId + '/installation', _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Retrieve context user's Facebook accounts
             * @returns {*}
             */
            getFacebookAccount: function (hint) {
                return jsonRequester.get(_cloudUrl + '/user/me/facebookaccount', null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Create facebook account
             * @param object
             * @param hint
             * @returns {*}
             */
            createFacebookAccount: function (object, hint) {
                return jsonRequester.post(_cloudUrl + '/user/me/facebookaccount', object, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Retrieve context user's Twitter accounts
             * @param hint
             * @returns {*}
             */
            getTwitterAccount: function (hint) {
                return jsonRequester.get(_cloudUrl + '/user/me/twitteraccount', null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Create twitter account
             * @param object
             * @param hint
             * @returns {*}
             */
            createTwitterAccount: function (object, hint) {
                return jsonRequester.post(_cloudUrl + '/user/me/twitteraccount', object, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            getRoles:function(userId, hint){
                if (!userId) {
                    throw new Error('Id can not be empty or null');
                }

                return jsonRequester.get(_cloudUrl + '/user/' + userId + '/role', null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            getRolesInAppRealm:function(userId, targetAppRealmId, hint){
                if (!userId) {
                    throw new Error('Id can not be empty or null');
                }
                if (!targetAppRealmId) {
                    throw new Error('TargetAppId can not be empty or null');
                }

                var params = {
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/user/' + userId + '/role' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            addRoles: function(userId, roleIds, hint){
                if (!userId) {
                    throw new Error('Id can not be empty or null');
                }
                if (!(roleIds instanceof Array)) {
                    throw new Error('Roles must be array');
                }

                return jsonRequester.post(_cloudUrl + '/user/' + userId + '/role', roleIds, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            addRolesInAppRealm: function(userId, roleIds, targetAppRealmId, hint){
                if (!userId) {
                    throw new Error('Id can not be empty or null');
                }
                if (!(roleIds instanceof Array)) {
                    throw new Error('Roles must be array');
                }
                if (!targetAppRealmId) {
                    throw new Error('TargetAppId can not be empty or null');
                }

                var params = {
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.post(_cloudUrl + '/user/' + userId + '/role' + queryParams, roleIds, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            removeRoles: function(userId, roleIds, hint){
                if (!userId) {
                    throw new Error('Id can not be empty or null');
                }
                if (!(roleIds instanceof Array)) {
                    throw new Error('Roles must be array');
                }
                var params = {
                    'roleIds': roleIds
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.delete(_cloudUrl + '/user/' + userId + '/role' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            removeRolesInAppRealm: function(userId, roleIds, targetAppRealmId, hint){
                if (!userId) {
                    throw new Error('Id can not be empty or null');
                }
                if (!(roleIds instanceof Array)) {
                    throw new Error('Roles must be array');
                }
                if (!targetAppRealmId) {
                    throw new Error('TargetAppId can not be empty or null');
                }

                var params = {
                    'roleIds': roleIds,
                    'targetAppRealmId':targetAppRealmId
                };

                var queryParams = util.encodeQuery(params);
                return jsonRequester.delete(_cloudUrl + '/user/' + userId + '/role' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            getSubscriptions:function(userId, hint){
                if (!userId) {
                    throw new Error('UserId can not be empty or null');
                }

                return jsonRequester.get(_cloudUrl + '/user/' + userId + '/subscription', null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            getSubscriptionsInAppRealm:function(userId, targetAppRealmId, hint){
                if (!userId) {
                    throw new Error('UserId can not be empty or null');
                }
                if (!targetAppRealmId) {
                    throw new Error('TargetAppId can not be empty or null');
                }

                var params = {
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/user/' + userId + '/subscription' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            addToSubscription: function(userId, subscriptionId, hint){
                if (!userId) {
                    throw new Error('UserId can not be empty or null');
                }
                if (!subscriptionId) {
                    throw new Error('SubscriptionId can not be empty or null');
                }

                var params = {
                    'subscriptionId': subscriptionId
                };

                return jsonRequester.post(_cloudUrl + '/user/' + userId + '/subscription', params, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            addToSubscriptionWithExpiration: function(userId, subscriptionId, expires, hint){
                if (!userId) {
                    throw new Error('UserId can not be empty or null');
                }
                if (!subscriptionId) {
                    throw new Error('SubscriptionId can not be empty or null');
                }
                if (!expires) {
                    throw new Error('Expires can not be empty or null');
                }

                var params = {
                    'subscriptionId': subscriptionId,
                    'expires': expires
                };

                return jsonRequester.post(_cloudUrl + '/user/' + userId + '/subscription', params, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            addToSubscriptionInAppRealm: function(userId, subscriptionId, targetAppRealmId, hint){
                if (!userId) {
                    throw new Error('UserId can not be empty or null');
                }
                if (!subscriptionId) {
                    throw new Error('SubscriptionId can not be empty or null');
                }
                if (!targetAppRealmId) {
                    throw new Error('TargetAppId can not be empty or null');
                }

                var params = {
                    'subscriptionId': subscriptionId,
                    'targetAppRealmId': targetAppRealmId
                };


                return jsonRequester.post(_cloudUrl + '/user/' + userId + '/subscription', params, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            addToSubscriptionWithExpirationInAppRealm: function(userId, subscriptionId, expires, targetAppRealmId, hint){
                if (!userId) {
                    throw new Error('UserId can not be empty or null');
                }
                if (!subscriptionId) {
                    throw new Error('SubscriptionId can not be empty or null');
                }
                if (!expires) {
                    throw new Error('Expires can not be empty or null');
                }
                if (!targetAppRealmId) {
                    throw new Error('TargetAppId can not be empty or null');
                }

                var params = {
                    'subscriptionId': subscriptionId,
                    'expires': expires,
                    'targetAppRealmId': targetAppRealmId
                };

                return jsonRequester.post(_cloudUrl + '/user/' + userId + '/subscription', params, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            removeFromSubscription: function(userId, subscriptionId){
                if (!userId) {
                    throw new Error('Id can not be empty or null');
                }
                if (!subscriptionId) {
                    throw new Error('SubscriptionId can not be empty or null');
                }

                return jsonRequester.delete(_cloudUrl + '/user/' + userId + '/subscription/' + subscriptionId, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            removeFromSubscriptionInAppRealm: function(userId, subscriptionId, targetAppRealmId, hint){
                if (!userId) {
                    throw new Error('Id can not be empty or null');
                }
                if (!subscriptionId) {
                    throw new Error('SubscriptionId can not be empty or null');
                }
                if (!targetAppRealmId) {
                    throw new Error('TargetAppId can not be empty or null');
                }

                var params = {
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.delete(_cloudUrl + '/user/' + userId + '/subscription/' + subscriptionId +'/' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            }

        };

        var dataProxy = {

            /**
             * Create a new object
             * @param className - Name of new object
             * @param object - JSON encoded object representing the object to be created
             * @param hint
             * @returns {*}
             */
            create: function (className, object, hint) {
                return jsonRequester.post(_cloudUrl + '/data/' + className, object, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Create a many new object
             * @param className - Name of new object
             * @param items - JSON encoded object representing the itmes to be created
             * @param hint
             * @returns {*}
             */
            createMany: function (className, items, hint){
                if (!(items instanceof Array)) {
                    throw new Error('Items must be array');
                }
                return jsonRequester.post(_cloudUrl + '/data/' + className + '/list', items, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Update Object by Id
             * To change the data on an object that already exists
             * @param className
             * @param id
             * @param object
             * @param hint
             * @returns updated object
             */
            update: function (className, id, object, hint) {
                return jsonRequester.put(_cloudUrl + '/data/' + className + '/' + id, object, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Update Object if exist or insert
             * To change the data on an object that already exists
             * @param className
             * @param object
             * @param hint
             * @returns updated object
             */
            updateExistOrInsert: function (className, object, hint) {
                return jsonRequester.put(_cloudUrl + '/data/' + className + '/upsert', object, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Partial Update by Criteria
             * The call accepts as a parameter a filter that identifies the data item(s) that will be the subjects of the update and an update definition.
             * Note: To figure out how to define compound filters and update definitions contact support@orionscloud.com.
             * @param className
             * @param filter
             * @param update
             * @param hint
             * @returns
             */
            updateByFilter: function (className, filter, update, hint) {
                var params = {
                    'filter': filterOptions.render(filter)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.put(_cloudUrl + '/data/' + className + queryParams, update, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Update One by Criteria
             * The call accepts as a parameter a filter that identifies the data item(s) that will be the subjects of the update and an update definition.
             * Note: To figure out how to define compound filters and update definitions contact support@orionscloud.com.
             * @param className
             * @param filter
             * @param update
             * @param hint
             * @returns
             */
            updateOneByFilter: function (className, filter, update, hint) {
                var params = {
                    'filter': filterOptions.render(filter)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.put(_cloudUrl + '/data/' + className +'/one'+ queryParams, update, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Retrieve Object by Id
             * Once you've created an object, you can retrieve its contents by sending a GET request to the object URL.
             * @param className
             * @param id
             * @param hint
             * @returns {*}
             */
            get: function (className, id, hint) {
                return jsonRequester.get(_cloudUrl + '/data/' + className + '/' + id, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            getPartial: function (className, id, projection, hint) {
                var params = {
                    'projection': projectionOptions.render(projection)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/data/' + className + '/' + id + '/partial' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Retrieve Objects by Id's
             * Once you've created an object, you can retrieve its contents by sending a GET request to the object URL.
             * @param className
             * @param ids
             * @param hint
             * @returns {*}
             */
            getByIds: function (className, ids, hint) {
                if (!(ids instanceof Array)) {
                    throw new Error('Ids must be array');
                }
                var params = {
                    'ids': ids
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/data/' + className + '/list' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /***
             * Retrieve Partial Objects by Id's
             * Once you've created an object, you can retrieve its contents by sending a GET request to the object URL.
             * @param className
             * @param ids
             * @param projection
             * @param hint
             * @returns {*}
             */
            getPartialByIds: function (className, ids, projection, hint) {
                if (!(ids instanceof Array)) {
                    throw new Error('Ids must be array');
                }
                var params = {
                    'ids': ids,
                    'projection': projectionOptions.render(projection)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/data/' + className + '/list/partial' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Search Objects
             * The call will retrieve a page of objects that match the search criteria sorted by the specified sort order.
             * Note: To learn how to define compound filters and sort orders contact support@orionscloud.com
             * @param className
             * @param filter
             * @param pageNumber
             * @param pageSize
             * @param sortOrder
             * @param hint
             * @returns {*}
             */
            getByFilter: function (className, filter, pageNumber, pageSize, sortOrder, hint) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'sortOrder': orderOptions.render(sortOrder)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/data/' + className + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            getPartialByFilter: function (className, filter, pageNumber, pageSize, sortOrder, projection, hint) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'sortOrder': orderOptions.render(sortOrder),
                    'projection': projectionOptions.render(projection)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/data/' + className + '/partial' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            getOneByFilter: function (className, filter, hint) {
                var params = {
                    'filter': filterOptions.render(filter)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/data/' + className + '/one' + queryParams , null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            getOnePartialByFilter: function (className, filter, projection, hint) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'projection': projectionOptions.render(projection)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/data/' + className +'/one/partial' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Delete Object by Id
             * @param className
             * @param id
             * @param hint
             * @returns {*}
             */
            deleteById: function (className, id, hint) {
                return jsonRequester.delete(_cloudUrl + '/data/' + className + '/' + id, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Delete Objects by Criteria
             * The call will delete the objects that match the search criteria.
             * Note: To learn how to define compound filters contact support@orionscloud.com
             * @param className
             * @param filter
             * @param hint
             * @returns {*}
             */
            deleteByFilter: function (className, filter, hint) {
                var queryParams = util.encodeQuery(filterOptions.render(filter));
                return jsonRequester.delete(_cloudUrl + '/data/' + className + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            getDistinct: function (className, filter, field, hint) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'field': field
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/data/' + className + '/distinct' + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var installation = {

            /**
             *  Create Installation
             * @param object
             * @returns {*}
             */
            create: function (object) {
                return jsonRequester.post(_cloudUrl + '/installation', object, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Update Installation by Id
             * @param id
             * @param object
             * @returns {*}
             */
            update: function (id, object) {
                return jsonRequester.put(_cloudUrl + '/installation/' + id, object, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Partial Update by Criteria
             * The call accepts as a parameter a filter that identifies the data item(s) that will be the subjects of the update and an update definition.
             * @param filter
             * @param update
             * @returns {*}
             */
            updateByFilter: function (filter, update) {
                var queryParams = util.encodeQuery(filterOptions.render(filter));
                return jsonRequester.put(_cloudUrl + '/installation' + queryParams, update, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Delete Installation by Id
             * @param id
             * @returns {*}
             */
            deleteById: function (id) {
                return jsonRequester.delete(_cloudUrl + '/installation/' + id, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Delete Installations by Criteria
             * @param filter
             * @returns {*}
             */
            deleteByFilter: function (filter) {
                var queryParams = util.encodeQuery(filterOptions.render(filter));
                return jsonRequester.delete(_cloudUrl + '/installation' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Retrieve Installation by Id
             * Once you've created an object, you can retrieve its by Id.
             * @param id
             * @returns {*}
             */
            get: function (id) {
                return jsonRequester.get(_cloudUrl + '/installation/' + id, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Search Installations
             * The call will retrieve a page of installations that match the search criteria sorted by the specified sort order.
             * Note: To learn how to define compound filters and sort orders contact support@orionscloud.com
             * @param filter
             * @param pageNumber
             * @param pageSize
             * @param sortOrder
             * @returns {*}
             */
            getByFilter: function (filter, pageNumber, pageSize, sortOrder) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'sortOrder': orderOptions.render(sortOrder)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/installation' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var purchaseProxy = {

            create: function (purchase) {
                if (!purchase) {
                    throw new Error('Purchase can not empty or null');
                }
                if(!purchase.Status) purchase.Status = 0;

                return jsonRequester.post(_cloudUrl + '/purchase', purchase, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            createInAppRealm: function (purchase, targetAppRealmId) {
                if (!purchase) {
                    throw new Error('Purchase can not empty or null');
                }
                if(!purchase.Status) purchase.Status = 0;

                var params = {
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.post(_cloudUrl + '/purchase' + queryParams, purchase, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            getById: function (id) {
                if (!id) {
                    throw new Error('Id can not empty or null');
                }
                return jsonRequester.get(_cloudUrl + '/purchase/'+id, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            getByIdInAppRealm: function (id, targetAppRealmId) {
                if (!id) {
                    throw new Error('Id can not empty or null');
                }
                var params = {
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/purchase/' + id + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            getByFilter: function (filter, pageNumber, pageSize, sortOrder) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'sortOrder': orderOptions.render(sortOrder)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/purchase' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            getByPartial: function (filter, pageNumber, pageSize, sortOrder, projection) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'sortOrder': orderOptions.render(sortOrder),
                    'projection': projectionOptions.render(projection)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/purchase' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            getByPartialInAppRealm: function (filter, pageNumber, pageSize, sortOrder, projection, targetAppRealmId) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'sortOrder': orderOptions.render(sortOrder),
                    'projection': projectionOptions.render(projection),
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/purchase' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            getByFilterInAppRealm: function (filter, pageNumber, pageSize, sortOrder, targetAppRealmId) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'sortOrder': orderOptions.render(sortOrder),
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/purchase' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            cancel: function (id) {
                if (!id) {
                    throw new Error('Id can not empty or null');
                }
                return jsonRequester.put(_cloudUrl + '/purchase/' + id + '/cancel', null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            cancelInAppRealm: function (id, targetAppRealmId) {
                if (!id) {
                    throw new Error('id can not empty or null');
                }
                if (!targetAppRealmId) {
                    throw new Error('targetAppRealmId can not empty or null');
                }
                var params = {
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.put(_cloudUrl + '/purchase/' + id + '/cancel'+ queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            delete: function (id) {
                if (!id) {
                    throw new Error('Id can not empty or null');
                }
                return jsonRequester.delete(_cloudUrl + '/purchase/' + id , null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            deleteInAppRealm: function (id, targetAppRealmId) {
                if (!id) {
                    throw new Error('id can not empty or null');
                }
                if (!targetAppRealmId) {
                    throw new Error('targetAppRealmId can not empty or null');
                }
                var params = {
                    'targetAppRealmId': targetAppRealmId
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.put(_cloudUrl + '/purchase/' + id + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var session = {

            /**
             * Create Session
             * When you store session objects to OrionsCloud there are 4 reserved fields that get automatically added:
             * Id (unique identifier of the object)
             * Created (date time the input was created)
             * Updated (date time the input was last updated)
             * Started (date time the input was started session)
             * Finished (date time the input was finished session)
             * @param object
             * @returns {*}
             */
            create: function (object) {
                return jsonRequester.post(_cloudUrl + '/session', object, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Update Sesion by Id
             * @param id
             * @param object
             * @returns {*}
             */
            update: function (id, object) {
                return jsonRequester.put(_cloudUrl + '/session/' + id, object, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Partial Update by Criteria
             * The call accepts as a parameter a filter that identifies the data item(s) that will be the subjects of the update and an update definition.
             * Note: To figure out how to define compound filters and update definitions contact support@orionscloud.com.
             * @param filter
             * @param object
             * @returns {*}
             */
            updateByFilter: function (filter, object) {
                var queryParams = util.encodeQuery(filterOptions.render(filter));
                return jsonRequester.put(_cloudUrl + '/session' + queryParams, object, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Delete Sessions by Id
             * @param id
             * @returns {*}
             */
            deleteById: function (id) {
                return jsonRequester.delete(_cloudUrl + '/session/' + id, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Delete Sessions by Criteria
             * The call will delete the objects that match the search criteria.
             * Note: To learn how to define compound filters contact support@orionscloud.com
             * @param filter
             * @returns {*}
             */
            deleteByFilter: function (filter) {
                var queryParams = util.encodeQuery(filterOptions.render(filter));
                return jsonRequester.delete(_cloudUrl + '/session' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Retrieve Sessions by Id
             * Once you've created an object, you can retrieve its by Id
             * @param id
             * @returns {*}
             */
            get: function (id) {
                return jsonRequester.get(_cloudUrl + '/session/' + id, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Search Sessions
             * The call will retrieve a page of objects that match the search criteria sorted by the specified sort order.
             * Note: To learn how to define compound filters and sort orders contact support@orionscloud.com
             * @param filter
             * @param pageNumber
             * @param pageSize
             * @param sortOrder
             * @returns {*}
             */
            getByFilter: function (filter, pageNumber, pageSize, sortOrder) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'sortOrder': orderOptions.render(sortOrder)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/session' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var contentProxy = {

            /***
             * This call will create Content with specific content type "MIME Type / Internet Media Type"
             * for instance: image/jpeg, video/mp4, video/mpeg.. etc
             * @param namespace
             * @param object
             * @param hint
             * @returns
             */
            create: function (namespace, object, hint) {
                return jsonRequester.post(_cloudUrl + '/content/' + namespace, object, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /***
             * This call will retrieve a content by Id
             * @param namespace
             * @param id
             * @param hint
             * @returns
             */
            get: function (namespace, id, hint) {
                return jsonRequester.get(_cloudUrl + '/content/' + namespace + '/' + id, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /***
             * This call will retrieve contents by Id
             * @param namespace
             * @param ids
             * @param hint
             * @returns
             */
            getList: function (namespace, ids, hint) {
                if (!(ids instanceof Array)) {
                    return new Error("Invalid ids. Must be array.");
                }
                var params = {
                    'ids': ids
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/content/' + namespace + queryParams, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /***
             * Update Content type
             * @param namespace
             * @param id
             * @param object
             * @param hint
             * @returns
             */
            update: function (namespace, id, object, hint) {
                return jsonRequester.put(_cloudUrl + '/content/' + namespace + '/' + id, object, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /***
             * Delete content by Id
             * @param namespace
             * @param id
             * @param hint
             * @returns
             */
            deleteById: function (namespace, id, hint) {
                return jsonRequester.delete(_cloudUrl + '/content/' + namespace + '/' + id, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /***
             * Delete all contents for current className
             * @param namespace
             * @param hint
             * @returns
             */
            deleteAll: function (namespace, hint) {
                return jsonRequester.delete(_cloudUrl + '/content/' + namespace, null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /***
             * Download byte array for content with position and count
             * @param namespace
             * @param id
             * @param position
             * @param count
             * @param success
             * @param error
             * @param hint
             * @returns
             */
            downloadByteArray: function (namespace, id, position, count, success, error, hint) {
                var params = {
                    'position': position,
                    'count': count
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.getByteArray(_cloudUrl + '/content/' + namespace + '/' + id + '/data' + queryParams, _accessToken, success, error, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /***
             * Append byte array in existing content
             * @param namespace
             * @param id
             * @param data
             * @param hint
             * @returns
             */
            appendByteArray: function (namespace, id, data, hint) {
                return jsonRequester.putByteArray(_cloudUrl + '/content/' + namespace + '/' + id + '/data', data, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            /***
             * Delete data in existing content
             * @param namespace
             * @param id
             * @param hint
             * @returns
             */
            deleteData: function (namespace, id, hint) {
                return jsonRequester.delete(_cloudUrl + '/content/' + namespace + '/' + id + '/data', null, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var busMessageProxy = {

            executeNoResult: function(operation, hint){

                if (!operation) {
                    throw new Error('Operation');
                }

                return jsonRequester.post(_cloudUrl + '/bus/message/novalue', operation, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            },

            executeWithResult: function(operation, hint){

                if (!operation) {
                    throw new Error('Operation');
                }

                return jsonRequester.post(_cloudUrl + '/bus/message/value', operation, _accessToken, hint)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var appVersionSettingsProxy = {

            /**
             * Retrieve App Version Settings for specific version
             * Once you've created an client settings via admin portal, you can retrieve its contents by sending a GET request to the object URL.
             * The app version will have to be in a header of your request in the following format
             * OrionsCloudDataAgent: App:AppName/AppVersion OS:OSName/OSVersion Device:DeviceName/DeviceModel
             * @returns {*}
             */
            get: function () {
                return jsonRequester.get(_cloudUrl + '/appversionsettings', null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var triggerProxy = {

            /**
             * This call will create a Trigger with specific context, operation and namespace
             * @param trigger
             * @returns {*}
             */
            queue: function (trigger) {
                if (!trigger) {
                    throw new Error('Trigger');
                }
                return jsonRequester.post(_cloudUrl + '/trigger', trigger, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var loggerProxy = {

            /**
             * Retrieve Log by Id
             * Once you've created an log, you can retrieve its by Id
             * @param id
             * @returns {*}
             */
            get : function(id){
                return jsonRequester.get(_cloudUrl + '/log/' + id, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Search Logs by criteria
             * The call will retrieve a page of log that match the search criteria sorted by the specified sort order.
             * Note: To learn how to define compound filters and sort orders contact support@orionscloud.com
             * @param filter
             * @param pageNumber
             * @param pageSize
             * @param sortOrder
             * @returns {*}
             */
            getByFilter: function (filter, pageNumber, pageSize, sortOrder) {
                var params = {
                    'filter': filterOptions.render(filter),
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'sortOrder': orderOptions.render(sortOrder)
                };
                var queryParams = util.encodeQuery(params);
                return jsonRequester.get(_cloudUrl + '/log' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Create Log
             * When you store session objects to OrionsCloud there are 4 reserved fields that get automatically added:
             * Id (unique identifier of the object)
             * Created (date time the log was created)
             * Updated (date time the log was last updated)
             * Started (date time the log was started)
             * Finished (date time the log was finished)
             * @param log
             * @returns {*}
             */
            log: function (log) {
                if(!log) throw Error('Log');
                if(!log.Level) log.Level = 0;
                if(!log.Status) log.Status = 0;
                return jsonRequester.post(_cloudUrl + '/log', log, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            logError: function (text, context) {
                var log = {
                    'Level':0,
                    'Status':0,
                    'Text': text,
                    'ComponentName':_componentName,
                    'ComponentVersion':_componentVersion,
                    'Context': context
                };
               this.log(log);
            },
            logWarning: function (text, context) {
                var log = {
                    'Level':1,
                    'Status':0,
                    'Text': text,
                    'ComponentName':_componentName,
                    'ComponentVersion':_componentVersion,
                    'Context': context
                };
                this.log(log);
            },
            logTrace: function (text, context) {
                var log = {
                    'Level':2,
                    'Status':0,
                    'Text': text,
                    'ComponentName':_componentName,
                    'ComponentVersion':_componentVersion,
                    'Context': context
                };
                this.log(log);
            },
            logEvent: function (text, context) {
                var log = {
                    'Level':3,
                    'Status':0,
                    'Text': text,
                    'ComponentName':_componentName,
                    'ComponentVersion':_componentVersion,
                    'Context': context
                };
                this.log(log);
            },

            /**
             * Update Log by Id
             * @param id
             * @param object
             * @returns {*}
             */
            update: function (id, object) {
                return jsonRequester.put(_cloudUrl + '/log/' + id, object, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Partial Update Log by Criteria
             * The call accepts as a parameter a filter that identifies the data item(s) that will be the subjects of the update and an update definition.
             * Note: To figure out how to define compound filters and update definitions contact support@orionscloud.com.
             * @param filter
             * @param object
             * @returns {*}
             */
            updateByFilter: function (filter, object) {
                var queryParams = util.encodeQuery(filterOptions.render(filter));
                return jsonRequester.put(_cloudUrl + '/log' + queryParams, object, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Delete Log by Id
             * @param id
             * @returns {*}
             */
            deleteById: function (id) {
                return jsonRequester.delete(_cloudUrl + '/log/' + id, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            /**
             * Delete Log by Criteria
             * The call will delete the objects that match the search criteria.
             * Note: To learn how to define compound filters contact support@orionscloud.com
             * @param filter
             * @returns {*}
             */
            deleteByFilter: function (filter) {
                var queryParams = util.encodeQuery(filterOptions.render(filter));
                return jsonRequester.delete(_cloudUrl + '/log' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var tokenProxy = {

            create: function (item) {

                if (!item) {
                    throw new Error('Item can not be empty or null');
                }

                return jsonRequester.post(settings.cloudUrl + '/token', item, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            encrypt: function (id) {

                if (!id) {
                    throw new Error('Id can not be empty or null');
                }

                var input = {
                    "id": id
                };

                return jsonRequester.post(settings.cloudUrl + '/token/encrypted', input, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            decrypt: function (value) {

                if (!value) {
                    throw new Error('Value can not be empty or null');
                }

                var input = {
                    "value": value
                };

                return jsonRequester.get(settings.cloudUrl + '/token/decrypted', input, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            activate: function (id) {

                var input = {
                    "id": id
                };

                return jsonRequester.post(settings.cloudUrl + '/token/active', input, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            revoke: function (id) {

                if (!id) {
                    throw new Error('Id can not be empty or null');
                }

                var input = {
                    "id": id
                };

                return jsonRequester.post(settings.cloudUrl + '/token/revoked', input, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            get: function (id) {

                if (!id) {
                    throw new Error('Input parameter id can not be empty or null');
                }

                return jsonRequester.get(settings.cloudUrl + '/token/' + id, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            getOne: function (filter) {

                if (!filter) {
                    throw new Error('Input parameter filter can not be empty or null');
                }

                var data = {
                    'filter': filterRender(filter)
                };
                var queryParams = util.encodeQuery(data);

                return jsonRequester.get(settings.cloudUrl + '/token/one' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            },

            getByFilter: function (filter, pageNumber, pageSize, sortOrder) {

                if (!filter) {
                    throw new Error('Input parameter filter can not be empty or null');
                }
                if (pageNumber < 0) {
                    throw new Error('Input parameter pageNumber can not be negative');
                }
                if (pageSize <= 0) {
                    throw new Error('Input parameter pageSize can not be negative or zero');
                }
                if (sortOrder == null) {
                    throw new Error('Input parameter sortOrder can not be empty or null');
                }
                var data = {
                    'pageNumber': pageNumber,
                    'pageSize': pageSize,
                    'filter': filterRender(filter),
                    'sortOrder': orderRender(sortOrder)
                };
                var queryParams = util.encodeQuery(data);
                return jsonRequester.get(settings.cloudUrl + '/token' + queryParams, null, _accessToken)
                    .then(function (response) {
                        return response;
                    });
            }
        };

        var common = (function(){

            "use strict";

            return{
                FilterBulder: filterOptions.filterBuilder,
                OrderBuilder: orderOptions.orderBuilder,
                ProjectionBuilder: projectionOptions.projectionBuilder,
                UpdateBuilder: updateOptions.updateBuilder
            };
        }());

        var client = (function(){

            "use strict";

            return {
                Settings: settings,
                UserProxy: userProxy,
                PurchaseProxy: purchaseProxy,
                DataProxy: dataProxy,
                InstallationProxy: installation,
                SessionProxy: session,
                ContentProxy: contentProxy,
                AppVersionSettingsProxy: appVersionSettingsProxy,
                LogProxy: loggerProxy,
                TokenProxy: tokenProxy,
                TriggerProxy: triggerProxy,
                BusMessageProxy: busMessageProxy
        };

        }());

        return {
            Common : common,
            Client : client
        };

    }());

    return {Cloud: Cloud};

}());

