using Monopost.BLL.SocialMediaManagement.Posting;
// See https://aka.ms/new-console-template for more information

var accessToken = "EAAa9xhnJKfcBOxW4d1STZAZA8LYF0rPHBkX6KT3TnHAB97dlo45FWUEat2EQLAL4b82ZCcsGNGqnFVYNThCaN0WNgZBNJNncPgeXVNOl2zfJivDs1RTYypmPZCgytt9ToavUYjxcfqpmW5NyJ1P37CtywduAeBdpOsYZBPLZBc4N2okJGee6IZCBpqGKJ52eM2RF";
var userId = "17841459372027912";
var imgbbApiKey = "e0a110edf2286c29a1a29bf2b6a257ad";

var instagramPoster = new InstagramPoster(accessToken, userId, imgbbApiKey);

// Upload images
// var imagePaths = new List<string> { "D:\\1.jpg", "D:\\2.jpg" };
var imagePaths = new List<string>();
var post = await instagramPoster.CreatePostAsync("text", imagePaths);

// Fetch metrics
// await instagramPoster.FetchLikesAndCommentsAsync(carouselId);