﻿namespace TestAspire.ApiService.DataTransferObjects;

public class DatasetDto
{
    public int Id { get; set; }
    public required IFormFile ImageFile { get; set; }
    public required string Name { get; set; }
}