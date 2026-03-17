using FluentValidation.TestHelper;
using UrlShortener.MVC.Models;
using UrlShortener.MVC.Validators;
using Xunit;

namespace UrlShortener.Tests
{
    public class ValidationTests
    {
        [Fact]
        public void LoginViewModel_ShouldHaveError_WhenEmailIsEmpty()
        {
            // Arrange
            var validator = new LoginViewModelValidator();
            var model = new LoginViewModel { Email = "", Password = "Test123" };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void LoginViewModel_ShouldHaveError_WhenEmailIsInvalid()
        {
            // Arrange
            var validator = new LoginViewModelValidator();
            var model = new LoginViewModel { Email = "notanemail", Password = "Test123" };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void RegisterViewModel_ShouldHaveError_WhenPasswordIsTooShort()
        {
            // Arrange
            var validator = new RegisterViewModelValidator();
            var model = new RegisterViewModel 
            { 
                Email = "test@example.com", 
                Password = "Abc123",  // < 8 chars
                ConfirmPassword = "Abc123"
            };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void RegisterViewModel_ShouldHaveError_WhenPasswordHasNoUppercase()
        {
            // Arrange
            var validator = new RegisterViewModelValidator();
            var model = new RegisterViewModel 
            { 
                Email = "test@example.com", 
                Password = "abcd1234",  // no uppercase
                ConfirmPassword = "abcd1234"
            };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void RegisterViewModel_ShouldHaveError_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var validator = new RegisterViewModelValidator();
            var model = new RegisterViewModel 
            { 
                Email = "test@example.com", 
                Password = "Abcd1234",
                ConfirmPassword = "DifferentPassword"
            };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
        }

        [Fact]
        public void CreateShortUrlViewModel_ShouldHaveError_WhenUrlIsEmpty()
        {
            // Arrange
            var validator = new CreateShortUrlViewModelValidator();
            var model = new CreateShortUrlViewModel { OriginalUrl = "" };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OriginalUrl);
        }

        [Fact]
        public void CreateShortUrlViewModel_ShouldHaveError_WhenUrlIsInvalid()
        {
            // Arrange
            var validator = new CreateShortUrlViewModelValidator();
            var model = new CreateShortUrlViewModel { OriginalUrl = "notavalidurl" };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OriginalUrl);
        }

        [Fact]
        public void CreateShortUrlViewModel_ShouldNotHaveError_WhenUrlIsValid()
        {
            // Arrange
            var validator = new CreateShortUrlViewModelValidator();
            var model = new CreateShortUrlViewModel { OriginalUrl = "https://www.example.com" };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.OriginalUrl);
        }
    }
}
