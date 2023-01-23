using MTCG.Models;

namespace MTCG_Tests
{
    public class UserTests
    {
        // reply should be 400 if either username or password or both are empty
        [Test, Order(1)]
        public void CreateUser_FailureEmptyPassword()
        {
            var mockEventArgs = new Mock<HttpSvrEventArgs>();
            mockEventArgs.Object.Payload = "{\"Username\":\"username\", \"Password\":\"\"}";
            var user = new User();
            user.CreateUser(mockEventArgs.Object);
            mockEventArgs.Verify(e => e.Reply(400, "Error occured while creating user."));
        }

        [Test, Order(2)]
        public void CreateUser_FailureEmptyUsername()
        {
            var mockEventArgs = new Mock<HttpSvrEventArgs>();
            mockEventArgs.Object.Payload = "{\"Username\":\"\", \"Password\":\"password\"}";
            var user = new User();
            user.CreateUser(mockEventArgs.Object);
            mockEventArgs.Verify(e => e.Reply(400, "Error occured while creating user."));
        }

        // reply should be 400 if the username provided in the path doesnt match the token
        [Test, Order(3)]
        public void UpdateUserData_AuthorizationError()
        {
            var mockEventArgs = new Mock<HttpSvrEventArgs>();
            mockEventArgs.Object.Path = "/users/kienboec";
            var user = new User();
            var userToken = new UserToken();
            userToken.LoggedInUser = "name";
            user.UpdateUserData(mockEventArgs.Object, userToken);
            mockEventArgs.Verify(e => e.Reply(400, "Authorization doesn't match request."));
        }

        [Test, Order(4)]
        // reply should not be an authorization error if the names match
        public void UpdateUserData_NoAuthorizationError()
        {
            var mockEventArgs = new Mock<HttpSvrEventArgs>();
            mockEventArgs.Object.Path = "/users/name";
            var user = new User();
            var userToken = new UserToken();
            userToken.LoggedInUser = "name";
            user.UpdateUserData(mockEventArgs.Object, userToken);
            mockEventArgs.Verify(e => e.Reply(400, "Authorization doesn't match request."), Times.Never);
        }
    }
}
