using BookTracker.Api.Domain;
using BookTracker.Api.Domain.Members;
using BookTracker.Api.Storage.Members;
using Microsoft.AspNetCore.Identity;

namespace BookTracker.Api.Application.Members.CreateMember;

public class CreateMemberCommandHandler(IMemberRepository memberRepository, IPasswordHasher<Member> passwordHasher) : IHandler
{
    private const int MinPasswordLength = 8;

    public async Task<CreateMemberResponse> Execute(CreateMemberRequest request)
    {
        MemberName name = new(request.Name);
        MemberEmail email = new(request.Email);

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new DomainException("Password is required.");

        if (request.Password.Length < MinPasswordLength)
            throw new DomainException($"Password must contain at least {MinPasswordLength} characters.");

        if (await memberRepository.EmailExistsAsync(email))
            throw new MemberEmailAlreadyExistsException();

        Member member = new()
        {
            Name = name,
            Email = email,
            Role = MemberRole.Member // allen met createMember (((FIRST TIME)))
        };

        member.PasswordHash = passwordHasher.HashPassword(member, request.Password);

        Member savedMember = await memberRepository.AddAsync(member);

        return new CreateMemberResponse
        {
            Id = savedMember.Id,
            Name = savedMember.Name.Value,
            Email = savedMember.Email.Value
        };
    }
}