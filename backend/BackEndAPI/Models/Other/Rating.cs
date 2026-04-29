using BackEndAPI.Models.Document;
using NHibernate.Criterion;

namespace BackEndAPI.Models.Other
{
    public class Rating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? DocId { get; set; }
        public RatingType RatingType { get; set; }

        public int QualityScore { get; set; }
        public int? ServiceScore { get; set; }
        public int? ShipScore { get; set; }
        public ODOC Document { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<RatingImage> Images { get; set; }
    }
    public enum RatingType
    {
        Order = 1,
        General = 2
    }
    public class RatingImage
    {
        public int Id { get; set; }
        public int RatingId { get; set; }
        public string ImageUrl { get; set; }
        public Rating Rating { get; set; }
    }
    public class RatingDto
    {
        public int Id { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public RatingType RatingType { get; set; }

        public int QualityScore { get; set; }
        public int? ServiceScore { get; set; }
        public int? ShipScore { get; set; }                                  
        public ODOC Document { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<RatingImage> Images  { get; set; }
    }
    public class CreateOrderRatingDto
    {
        public int OrderId { get; set; }
        public int QualityScore { get; set; }
        public int ServiceScore { get; set; }
        public int ShipScore { get; set; }
        public string Comment { get; set; }
        public List<IFormFile> Images { get; set; }
    }
    public class CreateGeneralRatingDto
    {
        public int QualityScore { get; set; }
        public int ServiceScore { get; set; }
        public int ShipScore { get; set; }
        public string Comment { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
