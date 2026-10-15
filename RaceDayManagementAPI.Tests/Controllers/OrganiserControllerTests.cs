using Microsoft.AspNetCore.Mvc;
using RaceDayManagementAPI.Controllers;
using RaceDayManagementAPI.Models;
using RaceDayManagementAPI.Tests.Fixtures;
using Xunit;

namespace RaceDayManagementAPI.Tests.Controllers
{
    public class OrganiserControllerTests : IClassFixture<RaceDayDBFixture>, IClassFixture<JwtServiceFixture>
    {
        private readonly RaceDayDBFixture _dbFixture;
        private readonly JwtServiceFixture _jwtFixture;
        private readonly OrganiserController _controller;

        public OrganiserControllerTests(RaceDayDBFixture dbFixture, JwtServiceFixture jwtFixture)
        {
            _dbFixture = dbFixture;
            _jwtFixture = jwtFixture;
            _controller = new OrganiserController(_jwtFixture.JwtService, _dbFixture.DbContext);
        }

        #region GetMyProfile Tests

        [Fact(DisplayName = "GetMyProfile_WithValidOrganiser_ShouldReturnProfile")]
        public void GetMyProfile_WithValidOrganiser_ShouldReturnProfile()
        {
            // Arrange
            int organiserUserId = 1; // Thabo is an organiser
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            // Act
            var result = _controller.GetMyProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var profile = okResult.Value;

            Assert.NotNull(profile);
            int organiserID = ControllerContextHelper.GetAnonymousProperty<int>(profile, "OrganiserID");
            string companyName = ControllerContextHelper.GetAnonymousProperty<string>(profile, "CompanyName");
            Assert.Equal(1, organiserID);
            Assert.Equal("Soweto Running Club", companyName);
        }

        [Fact(DisplayName = "GetMyProfile_WithNonOrganiser_ShouldReturnNotFound")]
        public void GetMyProfile_WithNonOrganiser_ShouldReturnNotFound()
        {
            // Arrange
            int participantUserId = 3; // Lindiwe is a participant, not an organiser
            var claims = _jwtFixture.CreateParticipantClaims(participantUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            // Act
            var result = _controller.GetMyProfile();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact(DisplayName = "GetMyProfile_ShouldReturnCompanyName")]
        public void GetMyProfile_ShouldReturnCompanyName()
        {
            // Arrange
            int organiserUserId = 2; // Susan
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            // Act
            var result = _controller.GetMyProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var profile = okResult.Value;
            string companyName = ControllerContextHelper.GetAnonymousProperty<string>(profile, "CompanyName");
            Assert.NotNull(companyName);
            Assert.NotEmpty(companyName);
        }

        [Fact(DisplayName = "GetMyProfile_ShouldReturnUserDetails")]
        public void GetMyProfile_ShouldReturnUserDetails()
        {
            // Arrange
            int organiserUserId = 1;
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            // Act
            var result = _controller.GetMyProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var profile = okResult.Value;
            var userInfo = ControllerContextHelper.GetAnonymousProperty<object>(profile, "User");

            Assert.NotNull(userInfo);
            Assert.True(ControllerContextHelper.HasProperty(userInfo, "Email"));
            Assert.True(ControllerContextHelper.HasProperty(userInfo, "FullName"));
            Assert.True(ControllerContextHelper.HasProperty(userInfo, "Phone"));
        }

        [Fact(DisplayName = "GetMyProfile_ShouldReturnCreatedAtAndUpdatedAt")]
        public void GetMyProfile_ShouldReturnCreatedAtAndUpdatedAt()
        {
            // Arrange
            int organiserUserId = 1;
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            // Act
            var result = _controller.GetMyProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var profile = okResult.Value;
            Assert.True(ControllerContextHelper.HasProperty(profile, "CreatedAt"));
            Assert.True(ControllerContextHelper.HasProperty(profile, "UpdatedAt"));
        }

        #endregion

        #region UpdateMyProfile Tests

        [Fact(DisplayName = "UpdateMyProfile_WithValidData_ShouldUpdateSuccessfully")]
        public void UpdateMyProfile_WithValidData_ShouldUpdateSuccessfully()
        {
            // Arrange
            int organiserUserId = 2;  // Use organiser 2 to avoid mutating organiser 1
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            var updateRequest = new UpdateOrganiserRequest
            {
                FullName = "Updated Susan",
                Phone = "083 777 7777",
                Email = "susan.updated@capetowncycles.co.za",
                CompanyName = "Cape Town Cycles Updated"
            };

            // Act
            var result = _controller.UpdateMyProfile(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Verify organiser was updated
            var user = RaceDayDB.Users.FirstOrDefault(u => u.UserID == organiserUserId);
            Assert.Equal("Updated Susan", user?.FullName);
            Assert.Equal("083 777 7777", user?.Phone);
            Assert.Equal("susan.updated@capetowncycles.co.za", user?.Email);

            var organiser = RaceDayDB.Organisers.FirstOrDefault(o => o.UserID == organiserUserId);
            Assert.Equal("Cape Town Cycles Updated", organiser?.CompanyName);
        }

        [Fact(DisplayName = "UpdateMyProfile_WithConflictingEmail_ShouldReturnConflict")]
        public void UpdateMyProfile_WithConflictingEmail_ShouldReturnConflict()
        {
            // Arrange
            int organiserUserId = 2;  // Use organiser 2; organiser 1's email is thabo@runningclub.co.za
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            // Try to update with email that belongs to organiser 1
            var updateRequest = new UpdateOrganiserRequest
            {
                Email = "thabo@runningclub.co.za" // This email belongs to user 1
            };

            // Act
            var result = _controller.UpdateMyProfile(updateRequest);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact(DisplayName = "UpdateMyProfile_WithOwnEmail_ShouldUpdateSuccessfully")]
        public void UpdateMyProfile_WithOwnEmail_ShouldUpdateSuccessfully()
        {
            // Arrange
            int organiserUserId = 2;  // Use organiser 2
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            var currentEmail = "susan@capetowncycles.co.za";
            var updateRequest = new UpdateOrganiserRequest
            {
                Email = currentEmail,
                CompanyName = "Cape Town Cycles - Updated"
            };

            // Act
            var result = _controller.UpdateMyProfile(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact(DisplayName = "UpdateMyProfile_WithPartialData_ShouldUpdateOnlyProvidedFields")]
        public void UpdateMyProfile_WithPartialData_ShouldUpdateOnlyProvidedFields()
        {
            // Arrange
            int organiserUserId = 2;  // Use organiser 2
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            var originalUser = RaceDayDB.Users.First(u => u.UserID == organiserUserId);
            var originalPhone = originalUser.Phone;
            var originalCompanyName = RaceDayDB.Organisers.First(o => o.UserID == organiserUserId).CompanyName;

            var updateRequest = new UpdateOrganiserRequest
            {
                FullName = "Susan Only Name"
                // Leave other fields null/empty
            };

            // Act
            var result = _controller.UpdateMyProfile(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedUser = RaceDayDB.Users.First(u => u.UserID == organiserUserId);
            Assert.Equal("Susan Only Name", updatedUser.FullName);
            Assert.Equal(originalPhone, updatedUser.Phone); // Phone should not change

            var updatedOrganiser = RaceDayDB.Organisers.First(o => o.UserID == organiserUserId);
            Assert.Equal(originalCompanyName, updatedOrganiser.CompanyName); // Company should not change
        }

        [Fact(DisplayName = "UpdateMyProfile_WithNonExistentOrganiser_ShouldReturnNotFound")]
        public void UpdateMyProfile_WithNonExistentOrganiser_ShouldReturnNotFound()
        {
            // Arrange
            int invalidUserId = 999;
            var claims = _jwtFixture.CreateOrganizerClaims(invalidUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            var updateRequest = new UpdateOrganiserRequest
            {
                CompanyName = "Non Existent Events"
            };

            // Act
            var result = _controller.UpdateMyProfile(updateRequest);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact(DisplayName = "UpdateMyProfile_ShouldUpdateUserTimestamp")]
        public void UpdateMyProfile_ShouldUpdateUserTimestamp()
        {
            // Arrange
            int organiserUserId = 2;  // Use organiser 2
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            var user = RaceDayDB.Users.First(u => u.UserID == organiserUserId);
            var originalUpdatedAt = user.UpdatedAt;

            System.Threading.Thread.Sleep(100); // Small delay to ensure timestamp changes

            var updateRequest = new UpdateOrganiserRequest
            {
                FullName = "Updated Susan Timestamp"
            };

            // Act
            var result = _controller.UpdateMyProfile(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedUser = RaceDayDB.Users.First(u => u.UserID == organiserUserId);
            Assert.True(updatedUser.UpdatedAt >= originalUpdatedAt);
        }

        [Fact(DisplayName = "UpdateMyProfile_ShouldUpdateOrganiserTimestamp")]
        public void UpdateMyProfile_ShouldUpdateOrganiserTimestamp()
        {
            // Arrange
            int organiserUserId = 2;  // Use organiser 2
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            var organiser = RaceDayDB.Organisers.First(o => o.UserID == organiserUserId);
            var originalUpdatedAt = organiser.UpdatedAt;

            System.Threading.Thread.Sleep(100);

            var updateRequest = new UpdateOrganiserRequest
            {
                CompanyName = "Cape Town Cycles - Timestamp Updated"
            };

            // Act
            var result = _controller.UpdateMyProfile(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedOrganiser = RaceDayDB.Organisers.First(o => o.UserID == organiserUserId);
            Assert.True(updatedOrganiser.UpdatedAt >= originalUpdatedAt);
        }

        [Fact(DisplayName = "UpdateMyProfile_ShouldUpdateCompanyName")]
        public void UpdateMyProfile_ShouldUpdateCompanyName()
        {
            // Arrange
            int organiserUserId = 2; // Susan
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            var newCompanyName = "Cape Town Cycling Adventures";
            var updateRequest = new UpdateOrganiserRequest
            {
                CompanyName = newCompanyName
            };

            // Act
            var result = _controller.UpdateMyProfile(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedOrganiser = RaceDayDB.Organisers.First(o => o.UserID == organiserUserId);
            Assert.Equal(newCompanyName, updatedOrganiser.CompanyName);
        }

        [Fact(DisplayName = "UpdateMyProfile_ShouldReturnUpdatedProfile")]
        public void UpdateMyProfile_ShouldReturnUpdatedProfile()
        {
            // Arrange
            int organiserUserId = 1;
            var claims = _jwtFixture.CreateOrganizerClaims(organiserUserId);

            _controller.ControllerContext.HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims };

            var updateRequest = new UpdateOrganiserRequest
            {
                CompanyName = "Updated for Response Test",
                Phone = "082 777 7777"
            };

            // Act
            var result = _controller.UpdateMyProfile(updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;

            Assert.True(ControllerContextHelper.HasProperty(response, "OrganiserID"));
            string companyName = ControllerContextHelper.GetAnonymousProperty<string>(response, "CompanyName");
            Assert.Equal("Updated for Response Test", companyName);
            Assert.True(ControllerContextHelper.HasProperty(response, "User"));
        }

        #endregion
    }
}
