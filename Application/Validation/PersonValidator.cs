using Domain.Entities.Person;
using FluentValidation;

namespace Application.Validation
{
    public class PersonValidator : AbstractValidator<Person>
    {
        public PersonValidator()
        {
            RuleFor(p => p.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(p => p.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(p => p.NationalCode)
                .NotEmpty().WithMessage("National code is required.")
                .Length(10).WithMessage("National code must be exactly 10 digits.")
                .Matches(@"^\d{10}$").WithMessage("National code must contain only digits.");

            RuleFor(p => p.BirthDate)
                .LessThan(DateTime.Now).WithMessage("Birth date must be in the past.");
        }
    }
}
