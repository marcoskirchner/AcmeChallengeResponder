using Microsoft.AspNetCore.Authentication;

namespace AcmeChallengeRestResponder.Security.SimpleBearer
{
    public class SimpleBearerOptions : AuthenticationSchemeOptions
    {
        public string SecurityToken { get; set; }

        public override void Validate()
        {
            if (!(SecurityToken?.Length >= 10))
            {
                throw new System.InvalidOperationException("The specified security token is too short. Use a secure random long string. ");
            }
        }
    }
}
