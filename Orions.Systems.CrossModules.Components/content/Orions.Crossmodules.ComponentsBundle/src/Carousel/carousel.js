window.Orions.Carousel = {}

window.Orions.Carousel.switchSlice = (instance, index) => {
    instance.invokeMethodAsync('Switched', index);
}

window.Orions.Carousel.initializeCarousel = (instance) => {
    $('#carouselExampleIndicators').carousel({ interval: 0 });

    $('#carouselExampleIndicators-prev').click(
        () =>
        {
            $('#carouselExampleIndicators').carousel('prev');
            var index = $('div.active').index();
            window.Orions.Carousel.switchSlice(instance, index);
        });
    $('#carouselExampleIndicators-next').click(
        () =>
        {
            $('#carouselExampleIndicators').carousel('next');
            var index = $('div.active').index();
            window.Orions.Carousel.switchSlice(instance, index);
        });
}

