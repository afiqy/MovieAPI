using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Amazon;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider.Model;
using System;
using System.Collections.Generic;

namespace MovieAPI.Services
{
    public class CognitoService : ICognitoService
    {
        private readonly IConfiguration _config;
        private readonly AmazonCognitoIdentityProviderClient _provider;

        public CognitoService(IConfiguration config)
        {
            _config = config;

            var awsAccessKeyId = _config["AWS:AccessKeyId"];
            var awsSecretAccessKey = _config["AWS:SecretAccessKey"];
            var region = RegionEndpoint.GetBySystemName(_config["AWS:Region"]);

            var credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey);
            _provider = new AmazonCognitoIdentityProviderClient(credentials, region);
        }

        public async Task<(bool IsSuccess, string Message, string CognitoUserId)> RegisterUserAsync(string email, string password)
        {
            var clientId = _config["AWS:ClientId"];
            var userPoolId = _config["AWS:UserPoolId"];

            try
            {
                var request = new SignUpRequest
                {
                    ClientId = clientId,
                    Username = email,
                    Password = password,
                    UserAttributes = new List<AttributeType>
                    {
                        new AttributeType
                        {
                            Name = "email",
                            Value = email
                        }
                    }
                };

                var response = await _provider.SignUpAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var cognitoUserId = response.UserSub;  

                    var confirmRequest = new AdminConfirmSignUpRequest
                    {
                        Username = email,
                        UserPoolId = userPoolId
                    };

                    await _provider.AdminConfirmSignUpAsync(confirmRequest);

                    return (true, "User registered and confirmed successfully.", cognitoUserId);
                }

                return (false, "User registration failed.", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(string IdToken, string AccessToken)> LoginAsync(string email, string password)
        {
            var clientId = _config["AWS:ClientId"];

            var authRequest = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                ClientId = clientId,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", email },
                    { "PASSWORD", password }
                }
            };

            try
            {
                var authResponse = await _provider.InitiateAuthAsync(authRequest);
                return (authResponse.AuthenticationResult.IdToken, authResponse.AuthenticationResult.AccessToken);
            }
            catch (NotAuthorizedException)
            {
                throw new Exception("Invalid credentials. Please check your email and password.");
            }
            catch (UserNotConfirmedException)
            {
                throw new Exception("User email is not verified. Please verify your email before logging in.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}");
            }
        }
    }
}
