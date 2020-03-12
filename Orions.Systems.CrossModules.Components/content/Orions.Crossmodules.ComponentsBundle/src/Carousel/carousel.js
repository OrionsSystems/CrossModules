window.Orions.Carousel = {}

window.Orions.Carousel.initializeCarousel = (instance) => {
    $('#carouselExampleIndicators').carousel({ interval: 0 });

    $('#carouselExampleIndicators-prev').click(
        () =>
        {
            $('#carouselExampleIndicators').carousel('prev');
        });
    $('#carouselExampleIndicators-next').click(
        () =>
        {
            $('#carouselExampleIndicators').carousel('next');
        });
}

