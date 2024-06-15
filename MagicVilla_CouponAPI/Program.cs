using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal API Endpoints ----------------

app.MapGet("/api/coupon", async (ICouponRepository _couponRepo, ILogger<Program> _logger) => {
    APIResponse response = new APIResponse();
    _logger.Log(LogLevel.Information, "Getting all Coupons");
    response.Result = await _couponRepo.GetAllAsync();
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;

    return Results.Ok(response);

}).Produces<APIResponse>(200);

app.MapGet("/api/coupon/{id:int}", async (int id, ICouponRepository _couponRepo) =>
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

}).WithName("GetCoupon").Produces<APIResponse>(200);

app.MapPost("/api/coupon", async (ICouponRepository _couponRepo, IMapper _mapper, IValidator<CouponCreateDTO> _validation,
    [FromBody] CouponCreateDTO couponCreateDTO) =>
{
    APIResponse response = new(){IsSuccess = false,StatusCode = HttpStatusCode.BadRequest};
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

}).WithName("CreateCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);

app.MapPut("/api/coupon", async (ICouponRepository _couponRepo, IMapper _mapper, IValidator<CouponUpdateDTO> _validation, [FromBody] CouponUpdateDTO couponUpdateDTO) =>
{
    APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

    var validationResult = await _validation.ValidateAsync(couponUpdateDTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }

    var existingCoupon = await _couponRepo.GetAsync(couponUpdateDTO.Name);

    if (existingCoupon != null)
    {
        response.ErrorMessages.Add("Coupon name already exists");
        return Results.BadRequest(response);
    }

    // Retrieve coupon
    Coupon couponFromStore = await _couponRepo.GetAsync(couponUpdateDTO.Id);

    if (couponFromStore == null)
    {
        response.ErrorMessages.Add("Coupon not found");
        return Results.NotFound(response);
    }

    // Update coupon
    couponFromStore.IsActive = couponUpdateDTO.IsActive;
    couponFromStore.Name = couponUpdateDTO.Name;
    couponFromStore.Percent = couponUpdateDTO.Percent;
    couponFromStore.LastUpdated = DateTime.Now;

    await _couponRepo.SaveAsync();

    // Form response object
    response.Result = _mapper.Map<CouponDTO>(couponFromStore);
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;

    return Results.Ok(response);

}).WithName("UpdateCoupon").Accepts<CouponUpdateDTO>("application/json").Produces<APIResponse>(200).Produces(400);


app.MapDelete("/api/coupon/{id:int}",async (ICouponRepository _couponRepo, int id) =>
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

});


///---------------------------------------
app.UseHttpsRedirection();

app.Run();

