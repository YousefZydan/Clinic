using Application.Dtos;
using Application.Helpers;
using Application.Interfaces;
using Application.Repository;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Services
{
    public class FavouriteService(IMapper _mapping,IGenericRepository<Favourite> _repo,IGenericRepository<Patient>_PatientRepo) : IFavouriteService
    {
        public async Task<Result<string>> AddToFav(FavouriteCreateDto input,string userId)
        {
            var patient = await _PatientRepo.GetByAsync(x => x.User.Id == userId);
            if(patient is null)
                return Result<string>.Fail("Patient not found");
            

            var newRec = _mapping.Map<FavouriteCreateDto, Favourite>(input);
            
            newRec.PatientId = patient.First().Id;

            try
            {
                await _repo.CreateAsync(newRec);
                return Result<string>.Success("Added to favourites successfully");
            }
            catch (Exception ex)
            {
                return Result<string>.Fail(ex.Message);

            }
        }


        public async Task<List<FavouriteDto>> GetUserFavourites(string UserId)
        {
          var records =  await _repo.GetByAsync(x => x.Patient!.User.Id == UserId);
          return _mapping.Map<List<Favourite>, List<FavouriteDto>>(records);
          
        }

        public async Task<Result<string>> RemoveFromFav(Guid Id)
        {
            var record = await _repo.FindByIdAsync(Id);
            if (record == null)
            {
                return Result<string>.Fail("Favourite record not found");
            }
          return await _repo.DeleteAsync(record);
        }
    }
}


