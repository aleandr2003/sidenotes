$(document).ready(function(){
	$('.commentator_labelContainer', '#book_header').live('click', function(event){
		event.preventDefault();
		$(this).parents('.commentator_container').toggleClass('showDropbox');
	});
	$('.commentator_container', '#book_header').live('hover', function(event) {
		event.preventDefault();
		if (event.type == 'mouseover' || event.type == 'mouseenter') {}
		if (event.type == 'mouseout' || event.type == 'mouseleave') {
			$(this).removeClass('showDropbox');
		}
	});
	
	var currentCommentatorsListType = $('.commentator_type_current', '#book_header');
	if (currentCommentatorsListType.size()) {
		currentCommentatorsListType = currentCommentatorsListType.children('.commentator_type_link').attr('href');
		currentCommentatorsListType = currentCommentatorsListType.split('#');
		currentCommentatorsListType = currentCommentatorsListType.length<2 ? currentCommentatorsListType[0] : currentCommentatorsListType[1];
		currentCommentatorsListType = $('#' + currentCommentatorsListType);
	}else{
		currentCommentatorsListType = $('.commentator_list:first-child', '#book_header');
	}
	$('.commentator_list', '#book_header').hide();
	currentCommentatorsListType.show();
	
	$('.commentator_type_link', '#book_header').live('click', function(event){
		event.preventDefault();
		var link = $(this).attr('href');
			link = link.split('#');
			link = link.length<2 ? link[0] : link[1];
		if ($('#'+link).size()) {
			$('.commentator_list').hide();
			$('#' + link).show()
		}
		$('.commentator_type_current').removeClass('commentator_type_current');
		$(this).parent('.commentator_type').addClass('commentator_type_current');
	});
});

