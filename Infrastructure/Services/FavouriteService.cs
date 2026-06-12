using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Repository;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class FavouriteService(IMapper _mapping, IGenericRepository<Favourite> _repo, UserManager<User> _user, ApplicationDbContext _context) : IFavouriteService
    {
        public async Task<Result<string>> AddToFav(FavouriteCreateDto input, string userId)
        {
            var user = await _user.FindByIdAsync(userId);
            if (user is null)
                return Result<string>.Fail("User not found");


            var newRec = _mapping.Map<FavouriteCreateDto, Favourite>(input);

            newRec.UserId = user.Id;

            if (_context.Favourites.Any(x => x.UserId == user.Id && x.DoctorId == input.DoctorId))
                return Result<string>.Fail("Doctor already in favourites");

            try
            {

                var x = await _repo.CreateAsync(newRec);
                if (x.Succeeded)
                {
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(x => x.Id == input.DoctorId);
                    if (doctor == null)
                        return Result<string>.Fail("Doctor not found");

                    doctor.IsFav = true;
                    await _context.SaveChangesAsync();
                    return Result<string>.Success("Added to favourites successfully");
                }
                else
                {
                    return Result<string>.Fail(x.Error ?? "Failed to add to favourites");
                }
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);

            }
        }


        public async Task<List<FavouriteDto>> GetUserFavourites(string UserId)
        {
            var records = _context.Favourites.Where(x => x.UserId == UserId);

            return await records.ProjectTo<FavouriteDto>(_mapping.ConfigurationProvider)
                               .ToListAsync();

        }

        public async Task<Result<string>> RemoveFromFav(Guid doctorId, string userId)
        {
            var record = await _context.Favourites.FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.UserId == userId);
            if (record == null)
            {
                return Result<string>.Fail("Favourite record not found");
            }

            var x = await _repo.DeleteAsync(record);

            if (x.Succeeded)
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(x => x.Id == doctorId);
                if (doctor == null)
                    return Result<string>.Fail("Doctor not found");

                var isFav = await _context.Favourites.AnyAsync(x => x.DoctorId == doctorId);
                if (!isFav)
                {
                    doctor.IsFav = false;
                    await _context.SaveChangesAsync();
                }
                return Result<string>.Success("Removed from favourites successfully");

            }
            return Result<string>.Fail("Failed to remove from favourites");

        }
    }
}




