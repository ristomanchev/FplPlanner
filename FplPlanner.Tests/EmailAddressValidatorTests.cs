using FplPlanner.Service.Logic;

namespace FplPlanner.Tests;

public class EmailAddressValidatorTests
{
    [Theory]
    [InlineData("ana@example.com")]
    [InlineData("marko.petrov@students.finki.ukim.mk")]
    [InlineData("a+fpl@gmail.com")]
    public void ValidAddresses_AreAccepted(string email)
    {
        Assert.True(EmailAddressValidator.IsValid(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ana")]
    [InlineData("ana@example")]            // no top-level domain
    [InlineData("@example.com")]
    [InlineData("ana@.com")]
    [InlineData(" ana@example.com")]       // surrounding whitespace
    [InlineData("Ana <ana@example.com>")]  // display name, not a plain address
    [InlineData("YOUR_GMAIL_ADDRESS")]     // unfilled placeholder
    public void InvalidAddresses_AreRejected(string? email)
    {
        Assert.False(EmailAddressValidator.IsValid(email));
    }
}
