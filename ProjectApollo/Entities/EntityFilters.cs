//   Copyright 2020 Vircadia
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;

using Project_Apollo.Configuration;
using Project_Apollo.Hooks;
using Project_Apollo.Registry;

namespace Project_Apollo.Entities
{
    public abstract class EntityFilters
    {
    }
    public class PaginationInfo : EntityFilters
    {
        private static readonly string _logHeader = "[PaginationInfo]";

        // Overall parameters
        private readonly int _pageNum = 1;   // The number of page to return
        private readonly int _perPage = 20;  // How many entries per page

        // Results from last filter() operation
        int _currentPage = 1;
        int _currentItem = 1;

        // Empty pager that passes everything
        public PaginationInfo() : base()
        {
            _pageNum = 1;
            _perPage = int.MaxValue;
        }
        public PaginationInfo(int pPageNum, int pPerPage) : base()
        {
            _pageNum = Tools.Clamp(pPageNum, 1, 1000);
            _perPage = Tools.Clamp(pPerPage, 1, 1000);
        }
        public PaginationInfo(RESTRequestData pReq) : base()
        {
            try
            {
                if (pReq.Queries.TryGetValue("page", out string pageString))
                {
                    _pageNum = Tools.Clamp(Int32.Parse(pageString), 1, 1000);
                }
                if (pReq.Queries.TryGetValue("per_page", out string perPageString))
                {
                    _perPage = Tools.Clamp(Int32.Parse(perPageString), 1, 1000);
                }
            }
            catch (Exception e)
            {
                Context.Log.Error("{0} Exception fetching parameters: {1}", _logHeader, e);
            }
        }

        /// <summary>
        /// Given an Enumeratable of some items, return an enumerable which
        /// is the paged portion of that list.
        /// </summary>
        /// <typeparam name="T">Type of enumerable to scan and to return</typeparam>
        /// <param name="pToFilter">An enumerable of things to paginate</param>
        /// <returns></returns>
        public IEnumerable<T> Filter<T>(IEnumerable<T> pToFilter)
        {
            _currentPage = 1;
            _currentItem = 1;
            foreach (T item in pToFilter)
            {
                if (_pageNum == _currentPage)
                {
                    yield return item;
                }
                if (++_currentItem > _perPage) {
                    _currentItem = 1;
                    if (++_currentPage > _pageNum)
                    {
                        // If we're past the requested page, we're done
                        break;
                    }
                }
            }
            yield break;
        }

        /// <summary>
        /// The HTML reply body can have extra top level fields which
        ///     describe what pagination happened.
        /// </summary>
        /// <param name="pBody"></param>
        public void AddReplyFields(ResponseBody pBody)
        {
        }
    }

    public class AccountFilterInfo
    {
        private static readonly string _logHeader = "[AccountFilterInfo]";
        readonly string _filter; // one of 'connections','friends',
        readonly string _status; // one of 'online',
        readonly string _search; // specific name to look for (wildcards?)

        // Empty filter that passes everything
        public AccountFilterInfo()
        {
        }
        public AccountFilterInfo(RESTRequestData pReq)
        {
            try
            {
                pReq.Queries.TryGetValue("filter", out _filter);
                pReq.Queries.TryGetValue("status", out _status);
                pReq.Queries.TryGetValue("search", out _search);
                if (Context.Params.P<bool>(AppParams.P_DEBUG_QUERIES))
                {
                    Context.Log.Debug("{0} AccountFilterInfo: filter={1}, status={2}, search={3}",
                                _logHeader, _filter, _status, _search);
                }
            }
            catch (Exception e)
            {
                Context.Log.Error("{0} Exception fetching parameters: {1}", _logHeader, e);
            }
        }

        /// <summary>
        /// Generate an enumerable of filtered accounts.
        /// </summary>
        /// <param name="pMustBeOnline">if 'true', all returned accounts are 'online'</param>
        /// <param name="pRequestingAcct">requesting account. Used for admin permissions and connective checks.
        ///     Optional. If not specified, connectivity will be false.</param>
        /// <returns></returns>
        public IEnumerable<AccountEntity> Filter(IEnumerable<AccountEntity> pAccounts, AccountEntity pRequestingAcct = null)
        {
            foreach (AccountEntity acct in pAccounts) {
                bool filtering = !String.IsNullOrEmpty(_filter);
                bool matchedFilter = false;
                bool statusing = !String.IsNullOrEmpty(_status);
                bool matchedStatus = false;
                bool searching = !String.IsNullOrEmpty(_search);
                bool matchedSearch = false;

                if (filtering)
                {
                    string[] pieces = _filter.Split(",");
                    foreach (var filterCheck in pieces)
                    {
                        if (matchedFilter) break;

                        switch (filterCheck)
                        {
                            case "online":
                                matchedFilter = acct.IsOnline;
                                break;
                            case "friends":
                                matchedFilter = pRequestingAcct != null && acct.IsFriend(pRequestingAcct);
                                break;
                            case "connections":
                                matchedFilter = pRequestingAcct != null && acct.IsConnected(pRequestingAcct);
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (statusing)
                {
                    string[] pieces = _status.Split(",");
                    foreach (var statusCheck in pieces)
                    {
                        if (matchedStatus) break;

                        switch (statusCheck)
                        {
                            case "online":
                                matchedStatus = acct.IsOnline;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (searching)
                {
                    // TODO: does this do wildcard things?
                    if (_search == acct.Username)
                    {
                        matchedSearch = true;
                    }
                }
                if ((!filtering && !statusing && !searching)    // if not doing any selection
                    || ((filtering && matchedFilter)            // if any of the filters matched
                        || (statusing && matchedStatus)
                        || (searching && matchedSearch)))
                {
                    yield return acct;
                }
            }
            yield break;
        }
    }

    /// <summary>
    /// Filter for an enumerable of AccountEntities.
    /// Normally, a user can only see a collection of accounts that they
    /// are connected to. This filter has logic for that and for other
    /// scope information.
    /// </summary>
    public class AccountScopeFilter
    {
        // private static readonly string _logHeader = "[AccountScopeInfo]";
        readonly AccountEntity _contextAccount;
        readonly bool _asAdmin = false;
        /// <summary>
        /// Create an account access filter
        /// </summary>
        /// <param name="pReq">Request structure so can check for 'asAdmin' query</param>
        /// <param name="pAccount">the account who is doing the filtering</param>
        public AccountScopeFilter(RESTRequestData pReq, AccountEntity pAccount)
        {
            _asAdmin = pReq.Queries.TryGetValue("asAdmin", out _);
            _contextAccount = pAccount;
        }

        public AccountScopeFilter(AccountEntity pAccount, bool pAsAdmin)
        {
            _asAdmin = pAsAdmin;
            _contextAccount = pAccount;
        }

        /// <summary>
        /// Generate an enumerable of filtered accounts.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AccountEntity> Filter(IEnumerable<AccountEntity> pAccounts)
        {
            // Don't do any filtering yet
            foreach (AccountEntity acct in pAccounts) {
                bool matched = false;
                if (_contextAccount != null)
                {
                    if (_asAdmin && _contextAccount.IsAdmin)
                    {
                        // Admin can see all accounts
                        matched = true;
                    }
                    else
                    {
                        // A user can only see accounts they are connected to
                        matched = _contextAccount.IsConnected(acct);
                    }
                }
                if (matched) yield return acct;
            }
            yield break;
        }
    }
}
