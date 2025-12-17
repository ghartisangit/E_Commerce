using E_Commerce.DTOs;
using FluentValidation;

namespace E_Commerce.Validators
{
    public class ReviewDtoValidator:AbstractValidator<ReviewDto>
    {
        public ReviewDtoValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Comment)
               .NotEmpty().WithMessage("Comment is required")
               .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters");
        }
    }
}
