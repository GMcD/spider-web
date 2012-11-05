/*
 * Copyright © Gary MacDonald 2011
 * $Author: scapa $
 * $Revision: 667 $
 * $Date: 2011-10-03 18:16:16 +0100 (Mon, 03 Oct 2011) $
 */

 /* Tree Nodes must have Id, Label and Link attributes. May have hasChildren and classes attributes.
  * classes attribute is a list of css classes, which determine icon, as well as style.
  * The service which provides the JSON list must have a single XXX string XXX parameter ( called
  * root below), which takes the value of the id of the parent tree node.
  */

;(function($) {

function load(settings, root, child, container) {
    $.ajax({
	    type: "POST",
	    url: settings.url,
        async: true,
	    data: '{"Id": "' + root + '"}',
	    contentType: "application/json; charset=utf-8",
	    dataType: "json",
	    cache: false,
	    success: function (msg) {
		    function createNode(parent) {
                var ps = parent.children('li').children('span[class=placeholder]').parent();
                if ( ps.length > 0 ) ps.first().remove();
			    var current = $("<li/>").attr("id", String(this.Id) || "0").html("<span href='" + this.Link + "'>" + this.Label + "</span>").appendTo(parent);
                current.data('obj', this);
			    if (this.classes) {
				    current.children("span").addClass(this.classes);
			    }
			    if (this.expanded) {
				    current.addClass("open");
			    }
			    if (this.hasChildren || this.children && this.children.length) {
				    var branch = $("<ul/>").appendTo(current);
				    if (this.hasChildren) {
					    current.addClass("hasChildren");
					    createNode.call({
                            classes:"placeholder",
						    Label:"&nbsp;"
					    }, branch);
				    }
                    if (this.children && this.children.length) {
					    $.each(this.children, createNode, [branch])
				    }
			    } else {
                    current.hover( function () { $(this).addClass("hover"); },  function () { $(this).removeClass("hover"); } );    
                    current.on('click', settings.toggle);
                }
		    }
		    $.each(msg.Result, createNode, [child]);
            $(container).treeview({add: child});
        },
	    error: function (xhr, ajaxOptions, thrownError) {
            // The div id=status is a error place holder required on the parent page - fix
	        $("#status").text(xhr.statusText).addClass("error");
	    },
        complete: function(jqXHR, textStatus){
        }
	});
}

var proxied = $.fn.treeview;
$.fn.treeview = function(settings) {
	if (!settings.url) {
		return proxied.apply(this, arguments);
	}
	var container = this;
	load(settings, "", this, container);
	var userToggle = settings.toggle;
	return proxied.call(this, $.extend({}, settings, {
		collapsed: true,
		toggle: function() {
			var $this = $(this);
			if ($this.hasClass("hasChildren")) {
				var childList = $this.removeClass("hasChildren").find("ul");
                // The this.id/this.name here is the parent child relationship key - in the <LI id="n"/> element
				load(settings, this.id, childList, container);
 			}
			if (userToggle) {
				userToggle.apply(this, arguments);
			}
		}
	}));
};

})(jQuery);