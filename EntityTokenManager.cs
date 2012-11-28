/*
Copyright (c) Microsoft Corporation
All rights reserved.
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the 
License at http://www.apache.org/licenses/LICENSE-2.0 
    
THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING 
WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 

See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
*/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace mammerla.ServerIntegration
{
    /// <summary>
    /// Store type to store tokens on behalf of.
    /// </summary>
    public enum TokenStoreType
    {
        Yammer = 0,
        SharePoint = 1
    }

    /// <summary>
    /// Type of token to store.
    /// </summary>
    public enum TokenType
    {
        AccessToken = 0,
        RequestToken = 1
    }

    /// <summary>
    /// A simple class for storing tokens for OAuth based services using the Entity Data Model under the covers to connect to a database.
    /// </summary>
	public class EntityTokenManager
    {
		private Dictionary<string, string> localTokensAndSecrets = new Dictionary<string, string>();
 
        public EntityTokenManager() 
        {
            
		}
        
        /// <summary>
        /// Retrieves a token secret given an original token.
        /// </summary>
        /// <param name="storeType">Type of store to connect to.</param>
        /// <param name="token">Content of token to match against.</param>
        /// <returns></returns>
        public string GetTokenSecret(TokenStoreType storeType, string token) 
        {
            if (this.localTokensAndSecrets.ContainsKey(token))
            {
                return this.localTokensAndSecrets[token];
            }

            TokenStore ts = new TokenStore(Utilities.GetEntityConnectionString());

            var tokens = ts.Tokens.Where(t => t.TokenContent == token & t.StoreTypeId == (int)storeType);

            // should probably do better enforcement here that there is only one response.
            foreach (var tokenItem in tokens)
            {
                this.localTokensAndSecrets[token] = tokenItem.Secret;

                return tokenItem.Secret;
            } 
            
            return null;
		}
        
        /// <summary>
        /// Gets an available access token for a particulare store.  This is probably a method you shouldn't be using in production code,
        /// as it will likely result in using the wrong/inappropriate token to peform operations.
        /// </summary>
        /// <param name="storeType">Type off store</param>
        /// <returns>Access token for the store.</returns>
        public String GetAnyAccessToken(TokenStoreType storeType)
        {
            TokenStore entities = new TokenStore(Utilities.GetEntityConnectionString());

            var tokens = entities.Tokens.Where(t => t.Type == (int)TokenType.AccessToken & t.StoreTypeId == (int)storeType);

            foreach (var tokenItem in tokens)
            {
                return tokenItem.TokenContent;
            }

            return null;
        }
        
        /// <summary>
        /// Gets an access token given a user identifier (user name.)
        /// </summary>
        /// <param name="storeType">Type of store to retrieve from.</param>
        /// <param name="userId">Identifier of the user in the store (e.g., a user's SID, or e-mail address, or something else that uniquely identifies a user.)</param>
        /// <returns>Access token if available, or null if not.</returns>
        public String GetAccessToken(TokenStoreType storeType, String userId)
        {
            TokenStore entities = new TokenStore(Utilities.GetEntityConnectionString());

            var tokens = entities.Tokens.Where(t => t.UserId == userId & t.Type == (int)TokenType.AccessToken & t.StoreTypeId == (int)storeType);

            foreach (var tokenItem in tokens)
            {
                return tokenItem.TokenContent;
            }

            return null;
        }

        public bool GetAccessTokenAndContent(TokenStoreType storeType, String userId, out String accessToken, out String contextContent)
        {
            TokenStore entities = new TokenStore(Utilities.GetEntityConnectionString());

            var tokens = entities.Tokens.Where(t => t.UserId == userId & t.Type == (int)TokenType.AccessToken & t.StoreTypeId == (int)storeType);

            foreach (var tokenItem in tokens)
            {
                accessToken = tokenItem.TokenContent;
                contextContent = tokenItem.ContextContent;

                return true;
            }

            accessToken = null;
            contextContent= null;

            return false;
        }

        public bool GetAccessTokenAndSecret(TokenStoreType storeType, String userId, out String accessToken, out String accessTokenSecret)
        {
            TokenStore entities = new TokenStore(Utilities.GetEntityConnectionString());

            var tokens = entities.Tokens.Where(t => t.UserId == userId & t.Type == (int)TokenType.AccessToken & t.StoreTypeId == (int)storeType);

            foreach (var tokenItem in tokens)
            {
                accessToken = tokenItem.TokenContent;
                accessTokenSecret = tokenItem.Secret;

                return true;
            }

            accessToken = null;
            accessTokenSecret = null;

            return false;
        }

        public void StoreNewToken(TokenType tokenType, TokenStoreType storeType, String userId, String token, String contextContent)
        {
            this.StoreNewToken(tokenType, storeType, userId, token, contextContent, null);
        }

        public void StoreNewToken(TokenType tokenType, TokenStoreType storeType, String userId, String token, String contextContent, String tokenSecret)
        {
			this.localTokensAndSecrets[token] = tokenSecret;

            // does the token already exist?
            TokenStore tokenStore = new TokenStore(Utilities.GetEntityConnectionString());

            var tokens = tokenStore.Tokens.Where(t => t.TokenContent == token & t.StoreTypeId == (byte)storeType & t.UserId == userId & t.Type == (int)TokenType.AccessToken);
            bool doSaveChanges = false;

            foreach (var tokenItem in tokens)
            {
                tokenItem.Secret = tokenSecret;
                tokenItem.UserId = userId;
                tokenItem.StoreTypeId = (byte)storeType;
                tokenItem.ContextContent = contextContent;
                doSaveChanges = true; 
            }

            if (doSaveChanges)
            {
                tokenStore.SaveChanges();
                return;
            }

            // go create a new token.
            Token tas = tokenStore.Tokens.CreateObject();
            tas.Id = Guid.NewGuid();
            tas.UserId =  userId;
            tas.TokenContent = token;
            tas.Secret = tokenSecret;
            tas.StoreTypeId = (byte)storeType;
            tas.ContextContent = contextContent;
            tas.Type = (int)tokenType;

            tokenStore.Tokens.AddObject(tas);
            tokenStore.SaveChanges();
        }

        public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, TokenStoreType storeType, String userId, string accessToken, string accessTokenSecret) 
        {
            TokenStore entities = new TokenStore(Utilities.GetEntityConnectionString());

            var tokens = entities.Tokens.Where(t => t.TokenContent == requestToken & t.StoreTypeId == (int)storeType);

            foreach (var tokenItem in tokens)
            {
                entities.DeleteObject(tokenItem);
            }

            tokens = entities.Tokens.Where(t => t.TokenContent == accessToken & t.StoreTypeId == (int)storeType);
            bool doSaveChanges = false;

            foreach (var tokenItem in tokens)
            {
                tokenItem.Secret = accessTokenSecret;
                tokenItem.UserId = userId;

                doSaveChanges = true;
            }

            if (doSaveChanges)
            {
                entities.SaveChanges();
                return;
            }

            // go create a new token.
            Token tas = entities.Tokens.CreateObject();
            tas.Id = Guid.NewGuid();
            tas.UserId = userId;
            tas.TokenContent = accessToken;
            tas.Secret = accessTokenSecret;
            tas.StoreTypeId = (byte)storeType;
            tas.Type = (int)TokenType.AccessToken;

            entities.Tokens.AddObject(tas);
            entities.SaveChanges();

            this.localTokensAndSecrets.Remove(requestToken);
            this.localTokensAndSecrets[accessToken] = accessTokenSecret;
		}


	}
}