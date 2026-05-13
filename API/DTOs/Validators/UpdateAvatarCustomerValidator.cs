using FastEndpoints;
using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Validators
{
    public class UpdateAvatarCustomerValidator : Validator<ResUpdateAvatarProfile>
    {
        public UpdateAvatarCustomerValidator() {
            RuleFor(x => x.AvatarFile)
                .NotNull().WithMessage("Vui lòng đính kèm file ảnh đại diện.");
            RuleFor(x => x.AvatarFile)
                .Must(x => x != null && x.Length <= 5 * 1024 * 1024)
                .WithMessage("Kích thước ảnh quá lớn. Vui lòng chọn ảnh dưới 5MB.");
            RuleFor(x => x.AvatarFile)
                .Must(x => x.ContentType.Equals("image/jpeg")
                       || x.ContentType.Equals("image/jpg")|| x.ContentType.Equals("image/png"))
                .WithMessage("Chỉ chấp nhận định dạng ảnh JPG hoặc PNG.");
        }
    }
}
