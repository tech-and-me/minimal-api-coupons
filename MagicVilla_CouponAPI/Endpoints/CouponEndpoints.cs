using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace MagicVilla_CouponAPI.Endpoints;

public static class CouponEndpoints
{
    public static void ConfigureCouponEndPoints(this WebApplication app)
    {
        app.MapGet("/api/coupon",GetAllCoupons).Produces<APIResponse>(200);

        app.MapGet("/api/coupon/{id:int}", GetCouponById).WithName("GetCoupon").Produces<APIResponse>(200);

        app.MapPost("/api/coupon", CreateCoupon).WithName("CreateCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);

        app.MapPut("/api/coupon", UpdateCoupon);

        app.MapDelete("/api/coupon/{id:int}",DeleteCoupon);
    }

    private async static Task<IResult> GetAllCoupons(ICouponRepository _couponRepo, ILogger<Program> _logger)
    {
        APIResponse response = new APIResponse();
        _logger.Log(LogLevel.Information, "Getting all Coupons");
        response.Result = await _couponRepo.GetAllAsync();
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;

        return Results.Ok(response);
    }

    private async static Task<IResult> GetCouponById(int id, ICouponRepository _couponRepo)
    {
        APIResponse response = new APIResponse();
        response.Result = await _couponRepo.GetAsync(id);

        if (response.Result == null)
        {
            response.ErrorMessages.Add("Id not found");
            return Results.NotFound(response);
        }

        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;

        return Results.Ok(response);
    }

    private async static Task<IResult> DeleteCoupon(ICouponRepository _couponRepo, int id)
    {
        APIResponse response = new() { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false };

        Coupon couponFromStore = await _couponRepo.GetAsync(id);
        if (couponFromStore == null)
        {
            response.ErrorMessages.Add("Not found");
            response.StatusCode = HttpStatusCode.NotFound;

            return Results.NotFound(response);

        }
        await _couponRepo.RemoveAsync(couponFromStore);
        await _couponRepo.SaveAsync();

        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.NoContent;

        return Results.Ok(response);
    }

    private async static Task<IResult> CreateCoupon(ICouponRepository _couponRepo, IMapper _mapper, IValidator<CouponCreateDTO> _validation,
            [FromBody] CouponCreateDTO couponCreateDTO)
    {
        APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
        var validationResult = await _validation.ValidateAsync(couponCreateDTO);

        if (!validationResult.IsValid)
        {
            response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
            return Results.BadRequest(response);
        }

        var existingCoupon = await _couponRepo.GetAsync(couponCreateDTO.Name);

        if (existingCoupon != null)
        {
            response.ErrorMessages.Add("Coupon Name already exists");
            return Results.BadRequest(response);
        }

        // Convert couponCreateDTO to coupon to add to DB
        Coupon coupon = _mapper.Map<Coupon>(couponCreateDTO);
        // coupon.Id = _db.Coupons.OrderByDescending(c => c.Id).FirstOrDefault()?.Id + 1 ?? 1;
        await _couponRepo.CreateAsync(coupon);
        await _couponRepo.SaveAsync();

        // Convert coupon to couponDTO to return as response
        CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

        // Form Response Object
        response.Result = couponDTO;
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.Created;

        return Results.Ok(response);
        //return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDTO);

    }

    private async static Task<IResult> UpdateCoupon(ICouponRepository _couponRepo, int id)
    {
        APIResponse response = new() { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false };

        Coupon couponFromStore = await _couponRepo.GetAsync(id);
        if (couponFromStore == null)
        {
            response.ErrorMessages.Add("Not found");
            response.StatusCode = HttpStatusCode.NotFound;

            return Results.NotFound(response);

        }
        await _couponRepo.RemoveAsync(couponFromStore);
        await _couponRepo.SaveAsync();

        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.NoContent;

        return Results.Ok(response);
    }
}

