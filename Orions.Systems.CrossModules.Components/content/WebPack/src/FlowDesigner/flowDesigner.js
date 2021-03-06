﻿//import $ from 'jquery';
//import 'bootstrap-colorpicker';

(function () {

	let defaultRefreshNodeStatusInMilisecond = 30000; //default is 30 sec

	var common = {};
	common.touches = false;
	common.components = [];
	common.theme = 'dark';
	common.callbacks = {};
	common.statics = {}; // cache for static content
	common.changes = {};
	common.changedtabs = false;

	var flow = {};
	flow.components = [];
	flow.connections = [];
	flow.designer = [];
	flow.loaded = false;
	flow.isReadOnly = 1;

	var flownotifications = [];
	var flownotified = false;

	var mdraggable = {};

	var settings = {};

	var operation = {};
	operation.designer = {};

	function checktouches(e) {
		common.touches = true;
		$(document).off('touchstart', checktouches);
	}

	function findItemById(data, id) {
		return data.find(function (el) {
			return el.id === id;
		});
	};

	function diagonal(x1, y1, x2, y2) {
		return 'M' + x1 + ',' + y1 + 'C' + ((x1 + x2) / 2) + ',' + y1 + ' ' + ((x1 + x2) / 2) + ',' + y2 + ' ' + x2 + ',' + y2;
	}

	function getTranslate(value) {
		if (value instanceof jQuery)
			value = value.attr('transform');
		var arr = value.substring(10, value.length - 1).split(/\s|,/);
		return {
			x: parseInt(arr[0]),
			y: parseInt(arr[1])
		}
	}

	function setState(value) {
		$('#designerstate').html(value);
		setBlinkState(value ? true : false);
	}

	function setBlinkState(isBlink) {
		if (typeof (isBlink) == typeof (true)) {
			$('.mainmenu').find('.highlight').toggleClass('blink', isBlink);
		}
	}

	function offsetter(evt) {

		var position = { x: 0, y: 0 };

		if (!evt)
			return position;

		if (evt.touches) {
			position.x = evt.touches[0].pageX;
			position.y = evt.touches[0].pageY;
		} else {
			position.x = evt.pageX;
			position.y = evt.pageY;
		}
		var parent = evt.target;
		while (parent.offsetParent) {
			position.x -= parent.offsetLeft;
			position.y -= parent.offsetTop;
			parent = parent.offsetParent;
		}

		return position;
	}

	function savescrollposition() {
		if (common.tab) {
			var el = $('.designer-scrollbar');
			var tmp = common.tabscroll['tab' + common.tab.id];
			var pos = { x: el.prop('scrollLeft'), y: el.prop('scrollTop') };
			if (!tmp || (tmp.x !== pos.x && tmp.y !== pos.y)) {
				//SET('common.tabscroll.tab' + common.tab.id, pos);
			}
		}
	}

	function staticContent(target, type, callback) {

		var key = type + '.' + target;

		if (common.statics[key]) {
			callback(common.statics[key], true);
			return;
		}

		//var id = GUID(15);
		var id = Date.now().toString();

		if (type === 'html') {
			common.statics[key] = 'html.' + target;
			var com = findItemById(common.components, target);
			var title = target;
			if (com)
				title = com.name;

			//TODO
			$('#flowsettings').append('<div data-jc-id="html.' + target + '" data-jc="form" data-jc-path="common.form" class="hidden"> <div data-jc-scope="settings.' + target + '"> <div class="padding bg-fade"> <div class="row"> <div class="col-md-4 m"> <div data-jc="textbox" data-jc-path="comname" data-jc-config="placeholder: Type a label for node"> Label </div> </div> <div class="col-md-4 m"> <div data-jc="textbox" data-jc-path="comreference" data-jc-config="placeholder: Type a reference for this instance">Reference</div> </div> <div class="col-md-4 m"> <div class="fs12">Color:</div> <div data-jc="colorselector" data-jc-path="comcolor"></div> </div> </div> </div> ' + response + '<br/> <div class="ui-center padding gray"><i class="fa fa-ban mr5"></i> No advanced configuration.</div> <br/> <div class="ui-form-buttons"> <div class="help nmt" style="margin-bottom:5px">The options will be applied immediately.</div> <div data-jc="validation" data-jc-path="?" style="width:100%"> <button name="submit" class="exec" data-exec="#flow.settings" disabled="disabled" style="width:100%"> APPLY SETTINGS </button> </div> </div> </div> </div>');

		} else {
			var response = callback();
			common.statics[key] = response;
		}

		callback(common.statics[key], false);
	}

	function html(component, value) {
		//var el = this.element;
		var el = component;
		if (value === undefined)
			return el.html();
		if (value instanceof Array)
			value = value.join('');
		var type = typeof (value);
		return value || type === 'number' || type === 'boolean' ? el.empty().append(value) : el.empty();
	}

	function createComponentMenu(remoteCommonComponents) {

		var groupMap = {};

		if (remoteCommonComponents && remoteCommonComponents instanceof Array) {

			let defGroup = "Default Group";

			$.each(remoteCommonComponents, function (index, element) {

				element.group = element.group || defGroup;

				var _menuData = [];

				if (groupMap[element.group]) {
					_menuData = groupMap[element.group];
				}

				_menuData.push(element);
				groupMap[element.group] = _menuData;
			});

		}

		// create menu
		var c = $('.components');
		$.each(groupMap, function (groupName, group) {
			c.append('<li class="group" data-search=""><i class="fa fa-folder-open"></i> ' + groupName + '</li>')

			//sort group by Title
			group.sort(function (a, b) {
				var p1 = a.title.toLowerCase();
				var p2 = b.title.toLowerCase();
				if (p1 < p2) return -1;
				if (p1 > p2) return 1;
				return 0;
			});

			$.each(group, function (index, element) {
				element.icon = element.icon || 'fa fa-square';
				element.color = element.color || '#e3e3e8';
				c.append('<li draggable="true" class="component" data-id="' + element.id + '"><div><i class="' + element.icon + '" style="color:' + element.color + '"></i> ' + element.title + '</div></li>')
			});
		});
	}

	function saveRemoteJsonConfiguration(flowJson) {

		// TODO 
	}

	function toggleMainMenu() {

		$(document.body).toggleClass('mainmenu-hidden', settings.isMinimizeMainMenu.get());

		if (settings.isMinimizeMainMenu.get() == true) {
			hideMainMenu();
			
		} else {
			showMainMenu();
			
		}

		settings.isMinimizeMainMenu.toggle();
	}

	function hideMainMenu() {

		$('#mainMenuBtnId i').removeClass('fa-chevron-left');
		$('#mainMenuBtnId i').addClass('fa-chevron-right');

		$(document.body).addClass('mainmenu-hidden');

		settings.isMinimizeMainMenu.set(true);
	}

	function showMainMenu() {

		$('#mainMenuBtnId i').removeClass('fa-chevron-right');
		$('#mainMenuBtnId i').addClass('fa-chevron-left');

		$(document.body).removeClass('mainmenu-hidden');

		settings.isMinimizeMainMenu.set(false);
	}

	function toggleCommonMenu() {

		if (settings.isCommonMinimized.get() == true) {
			showCommonMenu();
		} else {
			hideCommonMenu();
		}

		//settings.isCommonMinimized.toggle();
	}

	function hideCommonMenu() {

		$('.panel').css('margin-right', -($('.panel').width()));
		$('.body').css('margin-right', 0);

		$('#commonMenuBtnId i').removeClass('fa-chevron-right');
		$('#commonMenuBtnId i').addClass('fa-chevron-left');

		settings.isCommonMinimized.set(true);
	}

	function showCommonMenu() {

		$('.panel').css('margin-right', 0);
		$('.body').css('margin-right', $('.panel').width());

		$('#commonMenuBtnId i').removeClass('fa-chevron-left');
		$('#commonMenuBtnId i').addClass('fa-chevron-right');

		settings.isCommonMinimized.set(false);
	}

	function shownotifications(force) {
		var el = $('#panel-notification');
		if (force) {
			var msg = flownotifications.shift();
			if (msg) {
				el.find('div').html(msg);
				setTimeout(function () {
					shownotifications(true);
				}, 3000);
				if (!flownotified) {
					el.aclass('panel-notified');
					flownotified = true;
				}
			} else if (flownotified) {
				el.rclass('panel-notified');
				flownotified = false;
			}
		} else if (!el.hclass('panel-notified'))
			shownotifications(true);
	}

	function getIcons() {
		var classes = document.styleSheets[0].rules || document.styleSheets[0].cssRules;
		var icons = {};
		for (var x = 0; x < classes.length; x++) {
			var cls = classes[x];
			var sel = cls.selectorText;

			if (sel && sel.substring(0, 4) == '.fa-') {
				sel = sel.split(',');
				var val = cls.cssText ? cls.cssText : cls.cssText.style.cssText;
				var a = val.match(/[^\x00-\x7F]+/);
				if (a)
					sel.forEach(function (s) {
						s = s.trim();
						s = s.substring(4, s.indexOf(':')).trim();
						icons[s] = a.toString();
					});
			}
		}
		return icons;
	}

	function getSize(el) {
		var size = {};
		el = $(el);
		size.width = el.width();
		size.height = el.height();
		return size;
	}

	window.Orions.FlowDesigner = {};

	window.Orions.FlowDesigner.ToggleMainMenu = toggleMainMenu;
	window.Orions.FlowDesigner.HideMainMenu = hideMainMenu;
	window.Orions.FlowDesigner.ShowMainMenu = showMainMenu;

	window.Orions.FlowDesigner.ToggleCommonMenu = toggleCommonMenu;
	window.Orions.FlowDesigner.HideCommonMenu = hideCommonMenu;
	window.Orions.FlowDesigner.ShowCommonMenu = showCommonMenu;

	window.Orions.FlowDesigner.Settings = function (el) { operation.designer.settings(el); };
	window.Orions.FlowDesigner.Copy = function (el) { operation.designer.copy(el); };
	window.Orions.FlowDesigner.Paste = function (el) { operation.designer.paste(el); };
	window.Orions.FlowDesigner.Duplicate = function (el) { operation.designer.duplicate(el); };
	window.Orions.FlowDesigner.Remove = function (el) { operation.designer.remove(el); };

	window.Orions.FlowDesigner.ZoomIn = function () { operation.designer.zoomin(); };
	window.Orions.FlowDesigner.ZoomReset = function () { operation.designer.zoomreset(); };
	window.Orions.FlowDesigner.ZoomOut = function () { operation.designer.zoomout(); };

	window.Orions.FlowDesigner.Init = function (componentInstance, designData) {

		var events = {};
		var panel = $('.panel');
		var offsetX = 0;
		var noscrollbar = panel.find('.noscrollbar');
		var body = $('.body');
		var min = 340;
		var ssw = 0; // scrollbar width
		var old = 0;

		function setPanelWidth(w) {

			if (w !== old) {
				old = w;

				if (w < min) w = min;
				panel.width(w);
				body.css('margin-right', w);
				//noscrollbar.width(w + ssw);
			}
		}

		events.mup = function (e) {

			e.stopPropagation();
			e.preventDefault();

			var w = $(window);
			w.off('mousemove', events.mm);
			w.off('touchmove', events.tm);
			w.off('mouseup', events.mup);
			w.off('touchend', events.mup);
			var width = panel.width();
			setPanelWidth(width);
		};

		events.mm = function (e) { //mousemove
			e.stopPropagation();
			e.preventDefault();
			events.resize(e.pageX);
		};

		events.tm = function (e) {
			e.stopPropagation();
			e.preventDefault();
			events.resize(e.touches[0].pageX);
		};

		events.resize = function (pageX, touch) {

			var WW = window.innerWidth; // flow designer screen width 
			//var pW = panel.width();
			//var x = ((pageX - (WW - pW)) + pW) >> 0;

			var x = (WW - pageX - offsetX) >> 0;

			//var max = (WW / 1.85) >> 0;
			//var x = (WW - pageX - offsetX) >> 0;

			//var x = WW - designerWidth - mainMenuWidth 

			//if (x > max) x = max;
			if (x < min) x = min;

			setPanelWidth(x);
		};



		$('.panel-resize-handle').on('mousedown touchstart', function (e) {

			e.preventDefault();
			e.stopPropagation();

			var w = $(window);
			w.on('mousemove', events.mm);
			w.on('touchmove', events.tm);
			w.on('touchend', events.mup);
			w.on('mouseup', events.mup);


			if (e.touches && e.touches[0])
				offsetX = $(this).offset().left - e.touches[0].pageX;
			else
				offsetX = e.offsetX;

		});


		$(document).on('click', '#panel-notification', function () {
			shownotifications(true);
		});

		// Hybrid devices
		$(document).on('touchstart', checktouches);

		Array.prototype.flowConnection = function (index, id) {
			for (var i = 0; i < this.length; i++)
				if (this[i].index === index && this[i].id === id)
					return this[i];
		};

		// Load jQuery Extenstion
		(function () {
			$.fn.FIND = function (selector, many, callback, timeout) {

				if (typeof (many) === 'function') {
					timeout = callback;
					callback = many;
					many = undefined;
				}

				var self = this;
				var output = findcomponent(self, selector); //TODO
				if (typeof (callback) === 'function') {

					if (output.length) {
						callback.call(output, output);
						return self;
					}

					//TODO fixme !
					W.WAIT(function () {
						var val = self.FIND(selector, many);
						return val instanceof Array ? val.length > 0 : !!val;
					}, function (err) {
						// timeout
						if (!err) {
							var val = self.FIND(selector, many);
							callback.call(val ? val : W, val);
						}
					}, 500, timeout);

					return self;
				}

				return many ? output : output[0];
			};
			$.fn.SETTER = function (selector, name) {

				var self = this;
				var arg = [];
				var beg = selector === true ? 3 : 2;

				for (var i = beg; i < arguments.length; i++)
					arg.push(arguments[i]);

				if (beg === 3) {
					selector = name;
					name = arguments[2];
					self.FIND(selector, true, function (arr) {
						for (var i = 0, length = arr.length; i < length; i++) {
							var o = arr[i];
							if (typeof (o[name]) === 'function')
								o[name].apply(o, arg);
							else
								o[name] = arg[0];
						}
					});
				} else {
					var arr = self.FIND(selector, true);
					for (var i = 0, length = arr.length; i < length; i++) {
						var o = arr[i];
						if (typeof (o[name]) === 'function')
							o[name].apply(o, arg);
						else
							o[name] = arg[0];
					}
				}

				return self;
			};
			$.fn.scope = function () {

				if (!this.length)
					return null;

				var data = this.get(0).$scopedata;
				if (data)
					return data;
				var el = this.closest('[data-jc-scope]');
				if (el.length) {
					data = el.get(0).$scopedata;
					if (data)
						return data;
				}
				return null;
			};
			$.fn.rclass = function (a) {
				return a == null ? this.removeClass() : this.removeClass(a);
			};
			$.fn.rclass2 = function (a) {

				var self = this;
				var arr = (self.attr('class') || '').split(' ');
				var isReg = typeof (a) === 'object';

				for (var i = 0, length = arr.length; i < length; i++) {
					var cls = arr[i];
					if (cls) {
						if (isReg) {
							a.test(cls) && self.rclass(cls);
						} else {
							cls.indexOf(a) !== -1 && self.rclass(cls);
						}
					}
				}

				return self;
			};
			$.fn.rattr = function (a) {
				return this.removeAttr(a);
			};
			$.fn.rattrd = function (a) {
				return this.removeAttr('data-' + a);
			};
			$.fn.hclass = function (a) {
				return this.hasClass(a);
			};
			$.fn.tclass = function (a, v) {
				return this.toggleClass(a, v);
			};
			$.fn.attrd = function (a, v) {
				a = 'data-' + a;
				return v == null ? this.attr(a) : this.attr(a, v);
			};
			$.fn.aclass = function (a) {
				return this.addClass(a);
			};

			// Appends an SVG element
			$.fn.asvg = function (tag) {

				if (tag.indexOf('<') === -1) {
					var el = document.createElementNS('http://www.w3.org/2000/svg', tag);
					this.append(el);
					return $(el);
				}

				var d = document.createElementNS('http://www.w3.org/1999/xhtml', 'div');
				d.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg">' + tag + '</svg>';
				var f = document.createDocumentFragment();
				while (d.firstChild.firstChild)
					f.appendChild(d.firstChild.firstChild);
				f = $(f);
				this.append(f);
				return f;
			};
			$.fn.psvg = function (tag) {

				if (tag.indexOf('<') === -1) {
					var el = document.createElementNS('http://www.w3.org/2000/svg', tag);
					this.prepend(el);
					return $(el);
				}

				var d = document.createElementNS('http://www.w3.org/1999/xhtml', 'div');
				d.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg">' + tag + '</svg>';
				var f = document.createDocumentFragment();
				while (d.firstChild.firstChild)
					f.appendChild(d.firstChild.firstChild);
				f = $(f);
				this.prepend(f);
				return f;
			};
			$.fn.rescroll = function (offset, bottom) {
				var t = this;
				t.each(function () {
					var e = this;
					var el = e;
					el.scrollIntoView(true);
					if (offset) {
						var count = 0;
						while (el && el.scrollTop == 0 && count++ < 25) {
							el = el.parentNode;
							if (el && el.scrollTop) {

								var off = el.scrollTop + offset;

								if (bottom != false) {
									if (el.scrollTop + el.getBoundingClientRect().height >= el.scrollHeight) {
										el.scrollTop = el.scrollHeight;
										return;
									}
								}

								el.scrollTop = off;
								return;
							}
						}
					}
				});
				return t;
			};
		})();

		$(window).on('keydown', function (e) {

			debugger;

			if (e.target.tagName === 'BODY' && !common.form) {

				e.keyCode === 13 && (e.ctrlKey || e.metaKey) && operation.flow.apply();

				if (flow.selected && flow.selected.length) {
					// ctrl+d
					if (e.keyCode === 68 && (e.ctrlKey || e.metaKey)) {
						e.preventDefault();
						designer.duplicate();
						return;
					}
					// ctrl+c
					if (e.keyCode === 67 && (e.ctrlKey || e.metaKey)) {
						e.preventDefault();
						designer.copy(e.shiftKey);
						return;
					}
				}
				// ctrl+v
				if (flow.clipboard && e.keyCode === 86 && (e.ctrlKey || e.metaKey)) {
					e.preventDefault();
					designer.paste()
				}

			}
		});

		var formSettingsComponet;
		var header = null;
		var csspos = {};
		var formSettings = {

			make: function () {

				//TODO
				//formSettingsComponet = $('.ui-form-container');
				var self = this;

				var icon;

				if (config.icon)
					icon = '<i class="fa fa-' + config.icon + '"></i>';
				else
					icon = '<i></i>';

				$(document.body).append('<div id="{0}" class="hidden ui-form-container"><div class="ui-form-container-padding"><div class="ui-form" style="max-width:{1}px"><div class="ui-form-title"><button class="ui-form-button-close" data-path="{2}"><i class="fa fa-times"></i></button>{4}<span>{3}</span></div></div></div>'.format(self._id, config.width || 800, self.path, config.title, icon));

				var el = $('#' + self._id);
				el.find('.ui-form').get(0).appendChild(self.element.get(0));
				self.rclass('hidden');
				self.replace(el);

				header = self.virtualize({ title: '.ui-form-title > span', icon: '.ui-form-title > i' });

				self.event('scroll', function () {
					EMIT('scroll', self.name);
					EMIT('reflow', self.name);
				});

				self.find('button').on('click', function () {
					switch (this.name) {
						case 'submit':
							self.submit(self.hide);
							break;
						case 'cancel':
							!this.disabled && self[this.name](self.hide);
							break;
					}
				});

				config.enter && self.event('keydown', 'input', function (e) {
					e.which === 13 && !self.find('button[name="submit"]').get(0).disabled && setTimeout(function () {
						self.submit(self.hide);
					}, 800);
				});
			},
			hide: function () {
				self.set('');
			},
			resize: function () {
				if (!config.center || self.hclass('hidden'))
					return;
				var ui = self.find('.ui-form');
				var fh = ui.innerHeight();
				var wh = $(W).height();
				var r = (wh / 2) - (fh / 2);
				csspos.marginTop = (r > 30 ? (r - 15) : 20) + 'px';
				ui.css(csspos);
			},
			submit: function () {
				if (config.submit)
					EXEC(config.submit, self);
				else
					self.hide();
			},
			cancel: function () {
				config.cancel && EXEC(config.cancel, self);
				self.hide();
			},
			configure: function (key, value, init, prev) {
				if (init)
					return;
				switch (key) {
					case 'icon':
						header.icon.rclass(header.icon.attr('class'));
						value && header.icon.aclass('fa fa-' + value);
						break;
					case 'title':
						header.title.html(value);
						break;
					case 'width':
						value !== prev && self.find('.ui-form').css('max-width', value + 'px');
						break;
				}
			},
			setter: function (value) {

				setTimeout2('noscroll', function () {
					$('html').tclass('noscroll', !!$('.ui-form-container').not('.hidden').length);
				}, 50);

				var isHidden = value !== config.if;

				if (self.hclass('hidden') === isHidden)
					return;

				setTimeout2('formreflow', function () {
					EMIT('reflow', self.name);
				}, 10);

				if (isHidden) {
					self.aclass('hidden');
					self.release(true);
					self.find('.ui-form').rclass('ui-form-animate');
					W.$$form_level--;
					return;
				}

				if (W.$$form_level < 1)
					W.$$form_level = 1;

				W.$$form_level++;

				self.css('z-index', W.$$form_level * 10);
				self.element.scrollTop(0);
				self.rclass('hidden');

				self.resize();
				self.release(false);

				config.reload && EXEC(config.reload, self);
				config.default && DEFAULT(config.default, true);

				if (!isMOBILE && config.autofocus) {
					var el = self.find(config.autofocus === true ? 'input[type="text"],select,textarea' : config.autofocus);
					el.length && el.eq(0).focus();
				}

				setTimeout(function () {
					self.element.scrollTop(0);
					self.find('.ui-form').aclass('ui-form-animate');
				}, 300);

				// Fixes a problem with freezing of scrolling in Chrome
				setTimeout2(self.id, function () {
					self.css('z-index', (W.$$form_level * 10) + 1);
				}, 1000);
			}
		}

		//Form Settings
		$(document).on('click', '.ui-form-button-close', function () {
			// SET($(this).attr('data-path'), '');
		});

		$(document).on('click', '.ui-form-container', function (e) {
			var el = $(e.target);
			if (!(el.hclass('ui-form-container-padding') || el.hclass('ui-form-container')))
				return;
			var form = $(this).find('.ui-form');
			var cls = 'ui-form-animate-click';
			form.aclass(cls);
			setTimeout(function () {
				form.rclass(cls);
			}, 300);
		});

		$(document).on('click', '#saveNodeUpdateChanges', function (e) {

			var el = $(e.target);

			var nodeConfigId = $("input[name=nodeConfigId]").val();
			var nodeConfigOriginalName = $("input[name=nodeConfigOriginalName]").val();
			var nodeConfigName = $("#nodeUpdateNameId").val();
			var nodeColor = $("#mycp input").val();

			var nodeEl = $('.node_status_' + nodeConfigId);
			if (!nodeEl) return;

			var selectedNodeObj = $('.node_' + nodeConfigId);
			if (!selectedNodeObj) return;

			var selectedComponent = findItemById(flow.components, nodeConfigId);

			if (nodeConfigOriginalName !== nodeConfigName) {
				nodeEl.html(nodeConfigName);
				selectedComponent.state.text = nodeConfigName;
			}

			selectedComponent.state.color = nodeColor;

			selectedNodeObj.find('rect').attr('fill', nodeColor);
			selectedNodeObj.find('text').attr('fill', nodeColor);

		});

		$(document).on('click', '#saveGeneralDesignerSettings', function (e) {

			var el = $(e.target);

			var refreshNodeStatuses = $("#refreshNodeStatuses").val();
			var isClosedLeftMenu = $("#isClosedLeftMenu").is(":checked");
			var isClosedRightMenu = $("#isClosedRightMenu").is(":checked");

			var refreshNumber = parseInt(refreshNodeStatuses);
			if (typeof refreshNumber !== 'number' && !refreshNumber) return Error('Refresh status value is not number');

			cookie.createCookie(cookie.names.refreshNodeStatusesCookieName, refreshNumber);
			cookie.createCookie(cookie.names.isClosedLeftMenuCookieName, isClosedLeftMenu);
			cookie.createCookie(cookie.names.isClosedRightMenuCookieName, isClosedRightMenu);

			$('#workflowDesignerSettings').modal('hide');
		});

		function refreshTraffic() {

			var spinnerEl = $('.flowRefreshTrafic');
			spinnerEl.removeClass('hidden');

			componentInstance.invokeMethodAsync('LoadStatuses').then(function (data) {

				//console.log('Status result.. : ' + data);

				statuses = JSON.parse(data);
				setTimeout(function () { spinnerEl.addClass('hidden'); }, 500);

				if (!statuses) return;

				$('.node_traffic').each(function () {
					var el = $(this);
					var id = el.attr('data-id');

					var foundedStatus = statuses.find(function (item) {
						return item.nodeId === id;
					});

					if (foundedStatus) {
						//var stats = common.traffic[id];
						var input = 0;
						var output = 0;
						var inputc = 0;
						var outputc = 0;

						// if (stats && (stats.input || stats.output)) {
						//     input = ((stats.input / count) * 100 >> 0);
						//     output = ((stats.output / count) * 100 >> 0);
						//     inputc = stats.input;
						//     outputc = stats.output;
						// }

						var sum = input > output ? input : output;
						el.toggleClass('m1', sum < 25).toggleClass('m2', sum > 24 && sum < 50).toggleClass('m3', sum > 49 && sum < 70).toggleClass('m4', sum > 69);
						// el.find('text').html('IO: <tspan>' + inputc + 'x</tspan> &#8644; <tspan>' + outputc + 'x</tspan>');

						if (foundedStatus != null) {
							el.find('text.node_traffic_operational_status').html(foundedStatus.state);
							el.find('text.node_traffic_system_status').html(foundedStatus.systemStatusMessage);
							el.find('text.node_traffic_label_status').html(foundedStatus.statusMessage);
						}

						//el.find('text').html(foundedStatus.defaultStatusMessage);

						var rect = el.find('rect');
						var w = +rect.attr('data-width');
						var p = (w / 100) * sum;
						rect.attr('width', p);
					}
				});
			}, function (err) {
				spinnerEl.addClass('hidden');
			});

		}

		var MESSAGES = {};
		MESSAGES.apply = '<i class="fa fa-info-circle blue mr5"></i>You have to click the &quot;<b>APPLY</b>&quot; button to apply all changes.';

		$(document).on('click dblclick', '.component', function (e) {

			var self = $(e.target).parent();

			var component;
			if (self.length) {
				var componentId = self.data("id");
				component = common.components.find(function (el) {
					return el.id === componentId
				});
			}

			var htmlContent = 'No info!';
			if (component && component.html && component.html.length > 0) {
				htmlContent = component.html;
			}

			confirm.show(htmlContent, ['OK'], function (index) {
				return;
			});

		});

		$(document).on('touchstart', '[draggable]', function (e) {

			if (!common.touches)
				return;

			var el = $(e.target);
			var target = el.hasClass('component') ? el : el.closest('.component');
			var val = target.length ? common.components.findItem('id', target.attr('data-id')) : null;

			if (val) {
				val = CLONE(val);
				val.tab = common.tab.id;
			}

			mdraggable.drag = true;
			mdraggable.component = val;

			$('#tmpclone').empty();
			var clone = el.clone();
			clone.appendTo('#tmpclone');

			var t = e.originalEvent.touches[0];
			clone.css({ position: 'absolute', left: t.pageX, top: t.pageY, 'z-index': 5 });
			mdraggable.el = clone;

			val && designer.dragdrop(val);
			e.stopPropagation();

		});

		$(document).on('touchmove', '[draggable]', function (e) {

			if (!common.touches)
				return;

			if (mdraggable.drag) {
				var t = e.originalEvent.touches[0];
				mdraggable.x = t.pageX;
				mdraggable.y = t.pageY;
				mdraggable.el.css({ left: t.pageX, top: t.pageY });
				e.stopPropagation();
				e.preventDefault();
			}
		});

		$(document).on('touchend', '[draggable]', function (e) {

			if (!common.touches || !mdraggable.drag)
				return;

			e.stopPropagation();
			mdraggable.drag = false;
			$('#tmpclone').empty();

			if (mdraggable.x < 280)
				return;

			var d = designerComponet; // TODO Check me !
			var el = d.element;
			var off = el.offset();
			var x = mdraggable.x - off.left;
			var y = mdraggable.y - off.top;
			x += el.prop('scrollLeft');
			y += el.prop('scrollTop');
			var zoom = d.getZoom();

			on.designer.add(mdraggable.component, (x - 50) / zoom, (y - 30) / zoom, false);
		});

		$(document).on('dragstart', function (ev) {

			if (common.touches)
				return;

			var el = $(ev.target);
			var target = el.hasClass('component') ? el : el.closest('.component');


			var component;
			if (target.length) {
				var componentId = el.data("id");
				component = common.components.find(function (el) {
					return el.id === componentId
				});
			}

			if (component) {
				component = Object.assign({}, component);
			}

			if (component) {
				ev.originalEvent.dataTransfer.setData('text', '1');

				designer.dragdrop(component);
			}
		});

		var isConfirm,
			visibleConfirm = false,
			confirmComponent;
		var confirm = {
			make: function () {

				confirmComponent = $('#confirm');
				self = confirmComponent;

				self.aclass('ui-confirm hidden');

				self.on('click', 'button', function () {
					var result = parseInt($(this).attrd('index'));
					confirm.hide(result);
				});

				self.click(function (e) {
					var t = e.target.tagName;
					if (t !== 'DIV')
						return;
					var el = self.find('.ui-confirm-body');
					el.aclass('ui-confirm-click');
					setTimeout(function () {
						el.rclass('ui-confirm-click');
					}, 300);
				});

				$(window).on('keydown', function (e) {
					if (!visibleConfirm)
						return;
					var index = e.which === 13 ? 0 : e.which === 27 ? 1 : null;
					if (index != null) {
						self.find('button[data-index="{0}"]'.format(index)).trigger('click');
						e.preventDefault();
					}
				});
			},
			show: function (message, buttons, fn) {
				self = confirmComponent;
				self.callback = fn;

				var builder = [];

				for (var i = 0; i < buttons.length; i++) {
					var item = buttons[i];
					var icon = item.match(/"[a-z0-9-]+"/);
					if (icon) {
						item = item.replace(icon, '').trim();
						icon = '<i class="fa fa-' + icon.toString().replace(/"/g, '') + '"></i>';
					} else
						icon = '';
					builder.push('<button data-index="' + i + '">' + icon + '' + item + '</button>');
				}

				confirm.content('ui-confirm-warning', '<div class="ui-confirm-message">' + message.replace(/\n/g, '<br />') + '</div>' + builder.join(''));
			},
			hide: function (index) {
				self = confirmComponent;
				self.callback && self.callback(index);
				self.rclass('ui-confirm-visible');
				visibleConfirm = false;
				setTimeout(function () {
					$('html').rclass('noscrollconfirm');
					self.aclass('hidden');
				}, 1000);
			},
			content: function (cls, text) {
				self = confirmComponent;
				$('html').aclass('noscrollconfirm');
				//!isConfirm && self.html('<div><div class="ui-confirm-body"></div></div>');
				!isConfirm && html(self, '<div><div class="ui-confirm-body"></div></div>');
				self.find('.ui-confirm-body').empty().append(text);
				self.rclass('hidden');
				visibleConfirm = true;
				setTimeout(function () {
					self.aclass('ui-confirm-visible');
				}, 5);
			}
		};

		confirm.confirm = confirm.show;

		var _jsonConfiguration,
			_refreshNodeStatusInMilisecond,
			_isMinimizeMainMenu = false,
			_isCommonMinimized = false;
		settings = {
			jsonConfiguration: {
				get: function () { return _jsonConfiguration; },
				set: function (value) { _jsonConfiguration = value; }
			},
			refreshNodeStatusInMilisecond: {
				get: function () {

					var refreshTimeInMiliseconds = cookie.getCookie(cookie.names.refreshNodeStatusesCookieName);
					if (refreshTimeInMiliseconds) {
						_refreshNodeStatusInMilisecond = refreshTimeInMiliseconds;
						return refreshTimeInMiliseconds;
					}

					if (!_refreshNodeStatusInMilisecond) {
						return defaultRefreshNodeStatusInMilisecond;
					}
					return _refreshNodeStatusInMilisecond;
				},
				set: function (value) {
					if (typeof value !== 'number') return Error('Values is not number');

					_refreshNodeStatusInMilisecond = value;
				}
			},
			isMinimizeMainMenu: {
				get: function () {
					var isShowMainMenu = cookie.getCookie(cookie.names.isClosedLeftMenuCookieName);
					if (isShowMainMenu) _isMinimizeMainMenu = isShowMainMenu;
					return _isMinimizeMainMenu;
				},
				set: function (value) { _isMinimizeMainMenu = value; },
				toggle: function () { _isMinimizeMainMenu = !_isMinimizeMainMenu; }
			},
			isCommonMinimized: {
				get: function () {
					var showCommonMenu = cookie.getCookie(cookie.names.isClosedRightMenuCookieName);
					if (showCommonMenu) _isCommonMinimized = showCommonMenu;
					return _isCommonMinimized;
				},
				set: function (value) { _isCommonMinimized = value; },
				toggle: function () { _isCommonMinimized = !_isCommonMinimized; }
			},
		};

		var svg;
		var connection;
		var drag = {};
		var skip = false;
		var data, selected, dragdrop, container, lines, main, scroller, touch, anim;
		var move = { x: 0, y: 0, drag: false, node: null, offsetX: 0, offsetY: 0, type: 0, scrollX: 0, scrollY: 0, moveby: { x: 0, y: 0 } };
		var select = { x: 0, y: 0, active: false, origin: { x: 0, y: 0 }, items: [] };
		var selector;
		var zoom = 1;
		var animcache = {};
		var animrunning = {};
		var animtoken = 0;
		//var icons = getIcons();

		var designerComponet;
		designer = {
			findPoint: function (selector, x, y) {
				var arr = svg.find(selector);
				var o = svg.offset();

				x += o.left;
				y += o.top;

				for (var i = 0, length = arr.length; i < length; i++) {
					var el = arr[i];
					var off = el.getBoundingClientRect(); // returns the size of an element and its position relative to the viewport.
					var ax = x - off.width;
					var ay = y - off.height;
					if (off.left >= ax && x <= off.right && off.top >= ay && y <= off.bottom)
						return el;
				}
				return svg.get(0);
			},

			// Init designer !
			make: function () {

				designerComponet = {};
				designerComponet = $('.ui-designer');
				var self = designerComponet;

				scroller = self.parent();
				self.aclass('ui-designer');
				self.append('<div class="ui-designer-grid"><svg width="3000" height="3000"></svg></div>');
				//self.append('<div class="ui-designer-grid"><svg width="6000" height="6000"><defs><filter id="svgshadow" x="0" y="0" width="180%" height="180%"><feGaussianBlur in="SourceAlpha" stdDeviation="5"/><feOffset dx="2" dy="2" result="offsetblur"/><feComponentTransfer><feFuncA type="linear" slope="0.20"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter><pattern patternUnits="userSpaceOnUse" id="svggrid" x="0" y="0" width="150" height="150"><image width="150" height="150" xlink:href="images/flowDesigner/themedark.png" /></pattern></defs><g class="svggrid"><rect id="svggridbg" width="15000" height="15000" fill="url(#svggrid)" /></g></svg></div>');

				var tmp = self.find('svg');

				svg = $(tmp.get(0));
				main = svg.asvg('g');
				connection = main.asvg('path').attr('class', 'connection');
				lines = main.asvg('g');
				container = main.asvg('g');

				//anim = svg.asvg('g').attr('class', 'animations');
				//selector = svg.asvg('rect').attr('class', 'selector').attr('opacity', 0).attr('rx', 5).attr('ry', 5);
				self.resize();

				tmp.on('mouseleave', function (e) {
					if (!common.touches && move.drag)
						self.mup(e.pageX, e.pageY, 0, 0, e);
				});

				tmp.on('mousedown mousemove mouseup', function (e) {

					if (common.touches)
						return;

					if (select.active && !(e.ctrlKey || e.metaKey)) {
						select.active = false;
						selector.attr('height', 0).attr('width', 0).attr('opacity', 0);
						return;
					}

					var offset;

					if (e.type === 'mousemove') {
						if (move.drag) {
							offset = offsetter(e);
							self.mmove(e.pageX, e.pageY, offset.x, offset.y, e);
						}
						if ((e.ctrlKey || e.metaKey) && select.active) {
							self.selectormove(e);
						}
					} else {
						offset = offsetter(e);
						if (e.type === 'mouseup') {
							if ((e.ctrlKey || e.metaKey) && select.active)
								return self.selectorup(e);
							self.mup(e.pageX, e.pageY, offset.x, offset.y, e);
						} else
							self.mdown(e.pageX, e.pageY, offset.x, offset.y, e);
					}
				});

				tmp.on('touchstart touchmove touchend', function (evt) {

					if (!common.touches)
						return;

					var e = evt.touches[0];
					var offset;
					if (evt.type === 'touchmove') {
						offset = offsetter(evt);
						touch = evt;
						if (move.drag) {
							if (move.type === 2 || move.type === 3) {
								offset.x += move.scrollX;
								offset.y += move.scrollY;
							}
							self.mmove(e.pageX, e.pageY, offset.x, offset.y, evt);
							evt.preventDefault();
						}
					} else if (evt.type === 'touchend') {
						offset = offsetter(touch);
						e = touch.touches[0];

						if (move.type === 2 || move.type === 3) {
							offset.x += move.scrollX;
							offset.y += move.scrollY;
						}

						touch.target = move.node ? findPoint(move.node.hasClass('output') ? '.input' : '.output', move.tx, move.ty) : svg.get(0);
						self.mup(e.pageX, e.pageY, offset.x, offset.y, touch);
					} else {
						offset = offsetter(evt);
						move.scrollX = +scroller.prop('scrollLeft');
						move.scrollY = +scroller.prop('scrollTop');
						self.mdown(e.pageX, e.pageY, offset.x, offset.y, evt);
					}
				});

				$(window).on('keydown', function (e) {

					if (e.keyCode === 68 && (e.ctrlKey || e.metaKey) && selected) {
						e.preventDefault();
						self.duplicate();
						return;
					}

					if (e.target.tagName === 'BODY') {
						var step = e.shiftKey ? 100 : 0;
						if (e.keyCode === 38) {
							designer.move(0, -20 - step, e);
						} else if (e.keyCode === 40) {
							designer.move(0, 20 + step, e);
						} else if (e.keyCode === 39) {
							designer.move(20 + step, 0, e);
						} else if (e.keyCode === 37) {
							designer.move(-20 - step, 0, e);
						}
					}

					if ((e.keyCode !== 8 && e.keyCode !== 46) || !selected || self.disabled || e.target.tagName !== 'BODY')
						return;
					self.remove();
					e.preventDefault();
				});

				self.on('dragover dragenter drag drop', function (e) {

					//console.log(e.type);

					if (common.touches)
						return;

					if (!dragdrop)
						return;

					switch (e.type) {
						case 'dragenter':
							if (!dragdrop.input || !dragdrop.output)
								return;

							if (drag.conn) {
								drag.conn.removeClass('dropselection');
								drag.conn = null;
							}

							//if (e.target.nodeName === 'path')
							drag.conn = $(e.target).addClass('dropselection');

							break;

						case 'drop':
							var tmp = $(e.target);
							var is = drag.conn ? true : false;

							if (drag.conn) {
								drag.conn.removeClass('dropselection');
								drag.conn = null;
							}

							var off = self.offset();

							var x = e.pageX - off.left;
							var y = e.pageY - off.top;

							x += self.prop('scrollLeft');
							y += self.prop('scrollTop');

							loading.show();

							dragdrop.x = (x - 50) / zoom;
							dragdrop.y = (y - 30) / zoom;

							// create node
							var desingComponent = JSON.stringify(dragdrop);
							componentInstance.invokeMethodAsync('CreateNode', desingComponent).then(function (data) {

								res = JSON.parse(data);

								res.$component = dragdrop;

								res.title = res.title || dragdrop.title;
								res.output = res.output || dragdrop.output;
								res.input = res.input || dragdrop.input;

								if (typeof (res.state) === 'object' && res.state)
									res.state = { text: res.state.text || dragdrop.title, color: res.state.color || dragdrop.color };
								else
									res.state = { text: res.title || '', color: dragdrop.color };

								on.designer.add(res, res.x, res.y, false);
								loading.hide();
							}, function (err) {
								debugger;
								loading.hide();
							});

							break;
					}

					e.preventDefault();
				});

				self.mmove = function (x, y, offsetX, offsetY, e) {
					switch (move.type) {
						case 1:
							scroller.prop('scrollLeft', move.x - x).prop('scrollTop', move.y - y);
							return;
						case 2:
						case 3:
							var off = svg.offset();
							var tx = x - off.left;
							var ty = y - off.top;

							if (zoom !== 1) {
								tx = (tx / zoom);
								ty = (ty / zoom);
							}

							connection.attr('d', diagonal(move.x, move.y, tx, ty));
							move.tx = tx;
							move.ty = ty;
							break;
						case 5:
							// Current node
							designer.moveselected(offsetX + move.offsetX, offsetY + move.offsetY, e);
							return;
					}
				};

				self.mup = function (x, y, offsetX, offsetY, e) {
					var el = $(e.target);
					switch (move.type) {
						case 2:
							connection.attr('d', '');
							if (el.hasClass('input')) {

								var oindex = +move.node.attr('data-index');
								var output = move.node.closest('.node');
								var input = el.closest('.node');
								var iindex = el.attr('data-index');

								// var instance = flow.components.findItem('id', output.attr('data-id'));
								var instance = findItemById(flow.components, output.attr('data-id'));

								if (instance) {
									var id = input.attr('data-id');
									var is = false;

									if (instance.connections[oindex]) {
										//is exist connection
										var existConnection = instance.connections.find(function (conEl) {
											return conEl.id === id;
										})

										if (existConnection) {
											is = true;
										} else {
											instance.connections.push({ index: iindex, id: id });
										}

										// if (instance.connections[oindex].flowConnection(iindex, id))
										// {
										//     is = true;
										// }else
										// {
										//     instance.connections[oindex].push({ index: iindex, id: id });
										// }
									} else {
										instance.connections = [{ index: iindex, id: id }];
										setState(MESSAGES.apply);
									}

									!is && designer.connect(+iindex, oindex, output, input);
								}
							}
							break;
						case 3:
							connection.attr('d', '');
							if (el.hasClass('output')) {
								var oindex = +el.attr('data-index');
								var output = el.closest('.node');
								var input = move.node.closest('.node');
								var iindex = move.node.attr('data-index');
								var instance = flow.components.findItem('id', output.attr('data-id'));
								if (instance) {
									var id = input.attr('data-id');
									var is = false;
									if (instance.connections[oindex]) {

										//is exist connection
										var existConnection = instance.connections.find(function (conEl) {
											return conEl.id === id;
										})
										if (existConnection) {
											is = true;
										} else {
											instance.connections.push({ index: iindex, id: id });
										}
									} else
										instance.connections[oindex] = [{ index: iindex, id: id }];

									!is && designer.connect(+iindex, oindex, output, input);
								}
							}
					}
					move.type === 1 && savescrollposition();
					move.type = 0;
					move.drag = false;
				};

				self.mdown = function (x, y, offsetX, offsetY, e) {

					var el = $(e.target);
					var tmp;

					move.drag = true;
					move.moved = false;

					var mpos = offsetter(e);
					var off = svg.offset();
					// mouse position within svg
					move.mposx = mpos.x - off.left;
					move.mposy = mpos.y - off.top;

					if (e.target.tagName === 'svg') {
						move.x = x + scroller.prop('scrollLeft');
						move.y = y + scroller.prop('scrollTop');
						move.type = 1;
						move.node = null;
						self.select(null);
						return;
					}

					if (el.hasClass('output')) {
						// output point
						move.type = 2;
						move.node = el;
						tmp = getTranslate(el.closest('.node'));
						var x = tmp.x + (+el.attr('cx'));
						var y = tmp.y + (+el.attr('cy'));
						move.x = x;
						move.y = y;
					} else if (el.hasClass('input')) {
						// input point
						move.type = 3;
						move.node = el;
						tmp = getTranslate(el.closest('.node'));
						var x = tmp.x + (+el.attr('cx'));
						var y = tmp.y + (+el.attr('cy'));
						move.x = x;
						move.y = y;
					} else if (el.hasClass('node_connection')) {
						// connection
						move.type = 4;
						move.node = el;
						move.drag = false;
						self.select(el);
					} else if (el.hasClass('click')) {

						tmp = el.closest('.node').attr('data-id');
						move.drag = false;
						if (BLOCKED('click.' + tmp, 1000)) return;

						designer.click(tmp);
						EMIT('designer.click', tmp);
					} else {
						tmp = el.closest('.node');
						var ticks = Date.now();
						var same = false;

						if (move.node && tmp.get(0) === move.node.get(0)) {
							var diff = ticks - move.ticks;
							if (diff < 300) {
								//
								on.designer.settings(move.node.attr('data-id'));
								return;
							}
							same = true;
						}

						// node
						move.node = tmp;
						move.ticks = ticks;

						if (!move.node.length) {
							move.drag = false;
							return;
						}

						var transform = getTranslate(move.node);
						move.offsetX = transform.x - offsetX;
						move.offsetY = transform.y - offsetY;
						move.type = 5;
						!same && self.select(move.node);
					}
				};

				self.remove = function () {
					on.designer.selectable(null);
					var idconnection;
					if (selected.hasClass('node')) {
						idconnection = selected.attr('data-id');
						on.designer.rem(idconnection);
					} else {
						on.designer.remConnection(selected.attr('data-from'), selected.attr('data-to'), selected.attr('data-fromindex'), selected.attr('data-toindex'));
						selected.remove();
					}

					setState(MESSAGES.apply);
				};

				self.select = function (el) {

					if ((selected && !el) || (selected && selected.get(0) === el.get(0))) {
						selected.removeClass('selected');
						selected = null;
						on.designer.selectable(null);
						return;
					} else if (selected)
						selected.removeClass('selected');

					if (!el) {
						selected = null;
						return;
					}

					el.addClass('selected', true);
					selected = el;

					on.designer.selectable(selected);
					on.designer.select(selected.attr('data-id'));
				};

				self.duplicate = function () {

					on.designer.selectable(null);

					var component = findItemById(flow.components, flow.selected.attr('data-id'));
					if (!component) return;

					loading.show();

					// copy settings from original node
					var copyComponent = Object.assign({}, component)
					copyComponent.x += 50;
					copyComponent.y += 50;
					copyComponent.connections = [];
					var desingComponent = JSON.stringify(copyComponent);
					componentInstance.invokeMethodAsync('DuplicateNode', copyComponent.id, desingComponent).then(function (data) {

						dupCompoment = JSON.parse(data);

						dupCompoment.$component = copyComponent;
						dupCompoment.output = dupCompoment.output || copyComponent.output || copyComponent.$component.output;
						dupCompoment.input = dupCompoment.input || copyComponent.input || copyComponent.$component.input;

						dupCompoment.$component = component.$component;
						on.designer.add(dupCompoment, dupCompoment.x, dupCompoment.y, false);
						loading.hide();

					}, function (err) {
						debugger;
						loading.hide();
					});

				};
			},

			dragdrop: function (el) {
				dragdrop = el;
			},
			resize: function () {
				var self = designerComponet;

				var bodyEl = $('.body');
				var size = {
					width: bodyEl.width(),
					height: bodyEl.height()
				};

				size.height -= self.offset().top;
				self.css(size);
			},
			add: function (item) {

				var self = designerComponet;

				if (!item.component) return;

				self.find('.node_' + item.id).remove();

				var g = container.asvg('g');
				var err = item.errors ? Object.keys(item.errors) : [];
				g.attr('class', 'node node_unbinded selectable' + (err.length ? ' node_errors' : '') + ' node_' + item.id + (item.isnew ? ' node_new' : ''));
				g.attr('data-id', item.id);
				var rect = g.asvg('rect');
				//   g.asvg('text').attr('class', 'node_status node_status_' + item.id).attr('transform', 'translate(2,-8)').text((item.state ? item.state.text : '') || '').attr('fill', (item.state ? item.state.color : '') || 'gray');


				var icon = null;

				//if (item.$component && item.$component.icon) {
				//	icon = item.$component.icon;
				//	if (icon)
				//		icon = icons[icon];
				//}

				var body = g.asvg('g');
				var label = (item.name || item.reference) ? body.asvg('text').html((item.reference ? '<tspan>{0}</tspan> | '.format(item.reference) : '') + item.name || '').attr('class', 'node_label') : null;

				//var text = body.asvg('text').text(item.title).attr('class', 'node_name').attr('transform', 'translate(0, '+ label ? 14 : 5 +')'); // format(label ? 14 : 5)
				var transN = label ? 14 : 5;
				var text = body.asvg('text').text(item.$component.name).attr('class', 'node_name').attr('transform', 'translate(0, ' + transN + ')');

				var inputcolors = null;
				var input = 0;
				var outputcolors = null;
				var output = 0;

				if (item.input != null) {
					if (item.input instanceof Array) {
						inputcolors = item.input;
						input = inputcolors.length;
					} else
						input = item.input;
				} else if (item.$component.input instanceof Array) {
					inputcolors = item.$component.input;
					input = inputcolors.length;
				} else
					input = item.$component.input;

				if (item.output != null) {
					if (item.output instanceof Array) {
						outputcolors = item.output;
						output = outputcolors.length;
					} else
						output = item.output;
				} else if (item.$component.output instanceof Array) {
					outputcolors = item.$component.output;
					output = outputcolors.length;
				} else
					output = item.$component.output;


				var padding = 18;
				var radius = 7;

				var count = Math.max(output || 1, input || 1);
				var height = (count > 1 ? 16 : 30) + count * 22;
				var width = (Math.max(label ? label.get(0).getComputedTextLength() : 0, text.get(0).getComputedTextLength()) + 30) >> 0;

				body.attr('transform', 'translate(15, ' + ((height / 2) - 2) + ')'); //format((height / 2) - 2)
				rect.attr('width', width).attr('height', height).attr('rx', 4).attr('ry', 4).attr('fill', (item.state ? item.state.color : '') || 'gray').attr('class', 'rect');

				if (icon)
					width += 34;

				if (icon) {
					//g.asvg('text').attr('class', 'icon').attr('text-anchor', 'middle').text(icon).attr('transform', 'translate(20,' + ((height / 2) >> 0) + 6) + ')';
					g.asvg('line').attr('x1', 40).attr('x2', 40).attr('y1', 0).attr('y2', height).attr('class', 'iconline');
				}

				//rect.attr('width', width).attr('height', height).attr('rx', 4).attr('ry', 4).attr('fill', (item.state ? item.state.color : '') || 'gray').attr('class', 'rect');

				g.attr('data-width', width);
				g.attr('data-height', height);

				var points = g.asvg('g');
				var top = ((height / 2) - ((item.$component.input * padding) / 2)) + 10;


				top = ((height / 2) - ((input * 22) / 2)) + 10;
				for (var i = 0; i < input; i++) {
					var o = points.asvg('circle').attr('class', 'input').attr('data-index', i).attr('cx', 0).attr('cy', top + i * 22).attr('r', 8);
					if (inputcolors)
						o.attr('fill', inputcolors[i]);
					else
						o.attr('fill', common.theme === 'dark' ? 'white' : 'black');
				}

				top = ((height / 2) - ((output * 22) / 2)) + 10;
				for (var i = 0; i < output; i++) {
					var o = points.asvg('circle').attr('class', 'output').attr('data-index', i).attr('cx', width).attr('cy', top + i * 22).attr('r', 8);
					if (outputcolors)
						o.attr('fill', outputcolors[i]);
					else
						o.attr('fill', common.theme === 'dark' ? 'white' : 'black');
				}

				if (flow.isReadOnly === 0) {

					// 1 Line - Operational Status

					g.asvg('rect').attr('class', 'consumption').attr('width', (width - 5)).attr('height', 3).attr('transform', 'translate(2, ' + (height + 22) + ')').attr('fill', common.theme === 'dark' ? '#505050' : '#E0E0E0');
					var plus = g.asvg('g').attr('class', 'node_traffic').attr('data-id', item.id);

					plus.asvg('rect').attr('data-width', width - 5).attr('width', 0).attr('height', 3).attr('transform', 'translate(2, ' + height + 8 + ')');
					plus.asvg('text').attr('class', 'node_traffic_operational_status').attr('transform', 'translate(2,' + (height + 16) + ')').text('Status ...');

					// 2 Line - System Status

					//add 20 to height
					var heightOfset = 20;
					var plusHeight = heightOfset;
					g.asvg('rect').attr('class', 'consumption').attr('width', (width - 5)).attr('height', 3).attr('transform', 'translate(2, ' + (height + 22 + plusHeight) + ')').attr('fill', common.theme === 'dark' ? '#505050' : '#E0E0E0');
					plus.asvg('rect').attr('data-width', width - 5).attr('width', 0).attr('height', 3).attr('transform', 'translate(2, ' + height + 8 + ')');
					plus.asvg('text').attr('class', 'node_traffic_system_status').attr('transform', 'translate(2,' + (height + 16 + plusHeight) + ')').text('Status ...');

					// 3 Line - Label Status

					//add 20 to height
					plusHeight += heightOfset;
					g.asvg('rect').attr('class', 'consumption').attr('width', (width - 5)).attr('height', 3).attr('transform', 'translate(2, ' + (height + 22 + plusHeight) + ')').attr('fill', common.theme === 'dark' ? '#505050' : '#E0E0E0');
					plus.asvg('rect').attr('data-width', width - 5).attr('width', 0).attr('height', 3).attr('transform', 'translate(2, ' + height + 8 + ')');
					plus.asvg('text').attr('class', 'node_traffic_label_status').attr('transform', 'translate(2,' + (height + 16 + plusHeight) + ')').text('Status ...');

					// 4 Line -

					//add 20 to height
					// plusHeight += heightOfset;
					// g.asvg('rect').attr('class', 'consumption').attr('width', (width - 5)).attr('height', 3).attr('transform', 'translate(2, ' + (height + 22 + plusHeight) + ')').attr('fill', common.theme === 'dark' ? '#505050' : '#E0E0E0');
					// plus.asvg('rect').attr('data-width', width - 5).attr('width', 0).attr('height', 3).attr('transform', 'translate(2, ' + height + 8 + ')');
					// plus.asvg('text').attr('transform', 'translate(2,' + (height + 16 + plusHeight) + ')').text('Status ...');
				}

				g.attr('transform', 'translate(' + item.x + ',' + item.y + ')'); //format(item.x, item.y)

				if (item.$component.click) {
					var clicker = g.asvg('g').addClass('click');
					clicker.asvg('rect').addClass('click').attr('data-click', 'true').attr('transform', 'translate(' + width / 2 - 9 + ',' + height - 9 + ')').attr('width', 18).attr('height', 18).attr('rx', 10).attr('ry', 10);
					clicker.asvg('rect').addClass('click').attr('data-click', 'true').attr('transform', 'translate(' + width / 2 - 4 + ',' + height - 4 + ')').attr('width', 8).attr('height', 8).attr('rx', 8).attr('ry', 8);
				}

				if (!data) data = []; //TODO
				data[item.id] = item;
			},
			moveselected: function (x, y, e) {

				x = Math.round(x / 3) * 3;
				y = Math.round(y / 3) * 3;

				e.preventDefault();

				move.node.each(function () {

					var el = $(this);
					var id = el.attr('data-id');

					el.attr('transform', 'translate(' + x + ',' + y + ')');

					let found = $.map(flow.components, function (val) {
						return val.id === id ? val : null;
					});
					var instance = found[0];

					//var instance = flow.components.findItem('id', id);

					if (instance) {
						instance.x = x;
						instance.y = y;
					}

					lines.find('.from_' + id).each(function () {
						var el = $(this);
						var off = el.attr('data-offset').split(',');
						var x1 = +off[0] + x;
						var y1 = +off[1] + y;
						var x2 = +off[6];
						var y2 = +off[7];
						off[4] = x1;
						off[5] = y1;
						el.attr('data-offset', '' + off[0] + ',' + off[1] + ',' + off[2] + ',' + off[3] + ',' + off[4] + ',' + off[5] + ',' + off[6] + ',' + off[7] + '');
						el.attr('d', diagonal(x1, y1, x2, y2));
					});

					lines.find('.to_' + id).each(function () {
						var el = $(this);
						var off = el.attr('data-offset').split(',');
						var x1 = +off[4];
						var y1 = +off[5];
						var x2 = +off[2] + x;
						var y2 = +off[3] + y;
						off[6] = x2;
						off[7] = y2;
						el.attr('data-offset', '' + off[0] + ',' + off[1] + ',' + off[2] + ',' + off[3] + ',' + off[4] + ',' + off[5] + ',' + off[6] + ',' + off[7] + '');
						//el.attr('data-offset', '{0},{1},{2},{3},{4},{5},{6},{7}'.format(off[0], off[1], off[2], off[3], off[4], off[5], off[6], off[7]));
						el.attr('d', diagonal(x1, y1, x2, y2));
					});
				});
			},
			move: function (x, y, e) {

				var self = designerComponet;

				e.preventDefault();

				if (flow.selected) {
					// Caching
					if (flow.selected.get(0) !== self.allowedselected) {
						self.allowed = {};
						var find = function (com) {
							if (!com)
								return;
							self.allowed[com.id] = true;
							Object.keys(com.connections).forEach(function (index) {
								com.connections[index].forEach(function (item) {
									self.allowed[item.id] = true;
									find(findItemByid(flow.components, item.id));
								});
							});
						};
						find(findItemByid(flow.components, flow.selected.attr('data-id')));
						self.allowedselected = flow.selected.get(0);
					}
				} else
					self.allowed = null;

				self.find('.node_connection').each(function () {

					var el = $(this);
					if (self.allowed && !self.allowed[el.attr('data-to')] && !self.allowed[el.attr('data-from')])
						return;

					var off = el.attr('data-offset').split(',');

					if (self.allowed) {

						if (self.allowed[el.attr('data-from')]) {
							off[4] = +off[4] + x;
							off[5] = +off[5] + y;
							off[6] = +off[6];
							off[7] = +off[7];
						}

						if (self.allowed[el.attr('data-to')]) {
							off[4] = +off[4];
							off[5] = +off[5];
							off[6] = +off[6] + x;
							off[7] = +off[7] + y;
						}

					} else {
						off[4] = +off[4] + x;
						off[5] = +off[5] + y;
						off[6] = +off[6] + x;
						off[7] = +off[7] + y;
					}

					this.setAttribute('data-offset', '' + off[0] + ',' + off[1] + ',' + off[2] + ',' + off[3] + ',' + off[4] + ',' + off[5] + ',' + off[6] + ',' + off[7] + '');
					el.attr('d', diagonal(off[4], off[5], off[6], off[7]));
				});

				self.find('.node').each(function () {
					var el = $(this);
					if (self.allowed && !self.allowed[el.attr('data-id')])
						return;
					var offset = el.attr('transform');
					offset = offset.substring(10, offset.length - 1).split(',');
					var px = +offset[0] + x;
					var py = +offset[1] + y;
					el.attr('transform', 'translate(' + px + ',' + py + ')');
					var instance = findItemById(flow.components, el.attr('data-id'));
					if (instance) {
						instance.x = px;
						instance.y = py;
					}
				});
			},
			autoconnect: function (reset) {

				var self = designerComponet;

				reset && self.find('.node_connection').remove();

				for (var i = 0, length = flow.components.length; i < length; i++) {
					var instance = flow.components[i];
					var output = self.find('.node_' + instance.id);
					if (!output.length)
						continue;
					Object.keys(instance.connections).forEach(function (index) {

						var item = instance.connections[index];
						var hash = 'c' + index + '_' + instance.id + 'x' + item.id;
						var e = lines.find('.' + hash);
						if (e.length)
							return;
						var input = self.find('.node_' + item.id);
						input.length && output.length && designer.connect(+item.index, +index, output, input);

					});
				}
			},
			connect: function (iindex, oindex, output, input) {

				var a = output.find('.output[data-index="' + 0 + '"]');
				var b = input.find('.input[data-index="' + 0 + '"]');

				var tmp = getTranslate(output);
				var acx = +a.attr('cx');
				var acy = +a.attr('cy');
				var ax = tmp.x + acx;
				var ay = tmp.y + acy;

				tmp = getTranslate(input);
				var bcx = +b.attr('cx');
				var bcy = +b.attr('cy');
				var bx = tmp.x + bcx;
				var by = tmp.y + bcy;

				var aid = output.attr('data-id');
				var bid = input.attr('data-id');

				var attr = {};
				attr['d'] = diagonal(ax, ay, bx, by);
				attr['data-offset'] = '' + acx + ',' + acy + ',' + bcx + ',' + bcy + ',' + ax + ',' + ay + ',' + bx + ',' + by + '';
				attr['stroke-width'] = 3;
				attr['data-fromindex'] = iindex;
				attr['data-from'] = aid;
				attr['data-to'] = bid;
				attr['data-toindex'] = oindex;
				attr['class'] = 'node_connection selectable from_' + aid + ' to_' + bid + (flow.connections[aid + '#' + oindex + '#' + iindex + '#' + bid] ? '' : ' path_new');
				lines.asvg('path').attr(attr);
			},
			setter: function (value) {

				var self = designerComponet;

				if (skip) {
					skip = false;
					return;
				}

				if (!value)
					return;

				data = {};
				selected = null;

				lines.empty();
				container.empty();

				$.each(value, function (index, item) {
					designer.add(item);
				});

				value.length && designer.autoconnect(true);
				self.select(null);
			},
			getZoom: function () {
				return zoom;
			},
			zoom: function (val) {
				switch (val) {
					case 0:
						zoom = 1;
						break;
					case 1:
						zoom += 0.1;
						break;
					case -1:
						zoom -= 0.1;
						break;
				}
				main.attr('transform', 'scale(' + zoom + ')');
			},

			animclear: function () {
				animcache = {};
				animrunning = {};
			}
		};

		function arrayRemove(data, cb, value) {

			var self = data;
			var arr = [];
			var isFN = typeof (cb) === 'function';
			var isV = value !== undefined;

			for (var i = 0, length = self.length; i < length; i++) {
				if (isFN) {
					!cb.call(self, self[i], i) && arr.push(self[i]);
				} else if (isV) {
					self[i][cb] !== value && arr.push(self[i]);
				} else {
					self[i] !== cb && arr.push(self[i]);
				}
			}
			return arr;
		};

		var on = {
			designer: {
				refresh: function () {

					if (!flow.loaded)
						return;

					//TODO Fix Me!
				},
				add: function (component, x, y, is, cf, ct, toindex, duplicate, fromindex) {

					var obj = {};
					obj.component = component.$component.id;
					obj.$component = component;
					obj.$component.name = component.title || component.$component.title;
					obj.state = component.state || {};
					obj.x = x;
					obj.y = y;
					obj.tab = component.tab;
					obj.connections = {};
					//obj.id = Date.now().toString();
					obj.id = component.id;
					obj.isnew = true;
					obj.typeFull = component.typeFull;

					if (typeof (component.state) === 'object' && component.state)
						obj.state = { text: component.state.text, color: component.state.color };
					else
						obj.state = { text: obj.$component.name || '', color: '' };

					if (is) {
						obj.connections[0] = [ct];
						var a = findItemById(flow.components, cf);
						var target = a.connections[toindex].findItemById(ct);
						if (target) {
							//TODO
							// if (obj.$component.output)
							//     obj.connections[0] = [CLONE(target)];
							// target.id = obj.id;
							// setTimeout(function() {
							//     EMIT('designer.refresh');
							// }, 50);
						}
					}

					if (duplicate) {
						obj.options = duplicate.options;
						obj.name = duplicate.name;
						obj.output = duplicate.output;
						//obj.tab = common.tab.id;
					}

					flow.components.push(obj);
					flow.designer.push(obj);
					//!is && SETTER('designer', 'add', obj);
					!is && designer.add(obj);

					//EMIT('add.' + obj.component);

					setState(MESSAGES.apply);

					// TODO
				},
				rem: function (id) {

					if (!id) return;

					confirm.show('Are you sure you want to remove selected component?', ['Yes', 'Cancel'], function (index) {

						if (index === 1) return;

						//var instance = findItemById(flow.components, id);
						var instance = flow.components.find(function (el) {
							return el.id === id;
						});

						//TODO
						//EMIT('rem.' + instance.component, instance);
						//flow.components = flow.components.remove('id', id);
						//flow.designer = flow.designer.remove('id', id);
						flow.components = arrayRemove(flow.components, 'id', id);
						flow.designer = arrayRemove(flow.designer, 'id', id);

						// flow.components.forEach(function (component) {
						//     Object.keys(component.connections).forEach(function (key) {
						//         var connections = component.connections[key];
						//         component.connections[key] = $.grep(connections, function (item) {
						//             return item.id === id;
						//         });
						//         !component.connections[key].length && (delete component.connections[key]);
						//     });
						// });

						var el = $('.node_' + id);

						$('.node_connection').each(function () {
							var el = $(this);
							(el.attr('data-from') === id || el.attr('data-to') === id) && el.remove();
						});

						el.remove();
						setState(MESSAGES.apply);
						on.designer.unselect();
					});
				},
				settings: function (id) {

					var component = findItemById(flow.components, id);

					var model = Object.assign({}, component.$component.options);

					component.options && $.extend(true, model, component.options);
					model.comname = component.name;
					model.comcolor = component.color;
					model.comnotes = component.notes;
					model.comreference = component.reference;
					settings.$output = component.output;
					settings.$input = component.input;

					// open property node settings 
					componentInstance.invokeMethodAsync('OpenPropertyGrid', id).then(null, function (err) {
						throw new Error(err);
					});

					settings.$id = id;
				},
				selectable: function (el) {

					if (!el) return;

					flow.selected = el;
					$('#designerbuttons button').each(function (index) {
						var node = $(this);
						var disabled = index ? (el ? false : true) : (el && el.hasClass('node') ? false : true);

						if (this.name === 'paste')
							disabled = flow.clipboard == null;

						node.toggleClass('disabled', disabled);
					});
				},
				select: function (id) {
					if (!id)
						return;

					var component = findItemById(flow.components, id);

					if (component) {

						if (!component.id) {
							toastr.error('Save configuration before edit it.');
							return;
						}

						var nodeConfigOriginalName = component.state.text || component.$component.name;
						$('#nodeUpdateNameId').val(nodeConfigOriginalName);
						var nodeConfigId = component.id;
						$('#nodeConfigId').val(nodeConfigId);
						$('#nodeConfigOriginalName').val(nodeConfigOriginalName);
						var nodeColor = component.state.color || component.$component.color;

						$('.colorSp').removeAttr('Style');
						$('.colorSp').attr('Style', 'background-color:' + nodeColor + ';');

						$('#mycp input').val(nodeColor);

						$('.nodeSettingsId>span').html(nodeConfigId);

						var readme = component.$component.readme;
						$('.nodeSettingsReadme>p').html(readme || '');
					}
				},
				unselect: function () {
					on.designer.select(null);
				},
				click: function (id) {
					if (!id)
						return;
					var component = flow.components.findItem('id', id);
					EMIT('click.' + component.component, component); //TODO
					SETTER('websocket', 'send', { target: id, event: 'click' }); //TODO
					success();
				},
				addConnection: function (a, b) {
					setState(MESSAGES.apply);
				},
				remConnection: function (a, b, ti, fi) {
					var component = findItemById(flow.components, a);

					if (!component)
						return;
					var connections = component.connections[fi];
					if (!connections)
						return;

					connections = $.grep(connections, function (item) {
						return item.index === ti && item.id === b;
					});

					component.connections[fi] = connections;
					!connections.length && (delete component.connections[fi]);
					setState(MESSAGES.apply);

					setTimeout(function () {
						on.designer.unselect();
					}, 1000);
				}
			},

			resize: function () {

				//TODO fix me !!!!
				//$('.noscrollbar').each(function () {
				//	var el = $(this);
				//	var top = el.offset().top;
				//	var h = el.closest('.panel,.mainmenu').height();
				//	el.css('height', h - top);
				//});

			},

			changed: function (action, id) {

				id && (common.changes[id] = common.changes[id] || []);
				var changes = common.changes;
				switch (action) {
					case 'add':
						changes[id].push(action);
						break;
					case 'mov':
						// don't care if it's new component or already moved
						if (!~changes[id].indexOf('add') && !~changes[id].indexOf('mov'))
							changes[id].push(action);
						setTimeout2('change.move', function () {
							saveMoved();
						}, 1000);
						break;
					case 'rem':
						if (~changes[id].indexOf('add'))
							changes[id] = undefined; // remove if not on server yet
						else
							changes[id].push(action);
						break;
					case 'add.connection':
					case 'rem.connection':
						if (!~changes[id].indexOf('add')) // don't care if it's new component, it's taken care of above
							changes[id].push('conn');
						break;
					case 'tabs':
						common.changedtabs = true;
						break;
					case 'flow.clear':
						common.changedtabs = true;
						break;
				};
			},
		};

		operation = {
			flow: {
				apply: function () {

					confirm.show('Are you sure you want to apply current workflow?', ['Yes', 'Cancel'], function (index) {
						if (index === 1) return;

						$('.node_new').removeClass('node_new');
						$('.path_new').removeClass('path_new');

						//var body = Object.assign({}, flow); //CLONE

						// body.loaded = undefined;
						// body.designer = undefined;
						// body.traffic = undefined;
						// body.selected = undefined;
						// body.connections = undefined;

						// flow.connections = {};
						//
						// flow.components.forEach(function (item) {
						//     item.isnew = undefined;
						//     item.$component = findItemById(common.components, item.component);
						//     if(item.connections){
						//         Object.keys(item.connections).forEach(function (index) {
						//             var conn = item.connections[index];
						//             flow.connections[item.id + '#' + index + '#' + conn.index + '#' + conn.id] = true;
						//         });
						//     }
						//
						// });

						// body.components.forEach(function (item) {
						//     item.$component = undefined;
						// });

						// var message = {};
						// message.type = 'apply';
						// message.body = body;

						setState('');

						var flowJson = getDesignerJson();
						loading.show();
						saveRemoteJsonConfiguration(flowJson).then(function (result) {
							//loading.hide(300);

							location.reload(true)
						});

						//reload workflow or refresh page !
						//on.designer.refresh();
						//location.reload(true);
					});
				},
				settings: function () {
					var component = findItemById(flow.components, settings.$id);
					component.options = settings[component.component];

					var tmp_n = component.name;
					var tmp_r = component.reference;
					var tmp_c = component.color;

					component.name = component.options.comname;
					component.reference = component.options.comreference;
					component.color = component.options.comcolor;
					component.notes = component.options.comnotes;

					//TODO
					EMIT('save.' + component.component, component, component.options);

					var is_designer = tmp_c !== component.color || tmp_n !== component.name || tmp_n !== component.options.comname || tmp_r !== component.options.comreference || component.output !== settings.$output || component.input !== settings.$input;

					if (component.name !== component.options.comname)
						component.options.comname = component.name;

					if (component.reference !== component.options.comreference)
						component.options.comreference = component.reference;

					if (component.color !== component.options.comcolor)
						component.options.comcolor = component.color;

					if (component.notes !== component.options.comnotes)
						component.options.comnotes = component.notes;

					var is_server = is_designer ? true : STRINGIFY(component.options) !== settings.$backup;

					component.options.comoutput = component.output;
					component.options.cominput = component.input;

					if (!component.isnew && is_server) {
						//SETTER('websocket', 'send', { target: settings.$id, type: 'options', body: component.options });
						success();
					}

					if (component.output != null) {
						var count = component.output instanceof Array ? component.output.length : component.output;
						Object.keys(component.connections).forEach(function (key) {
							var index = +key;
							index >= count && (delete component.connections[key]);
						});
					}

					if (component.input != null) {
						var count = component.input instanceof Array ? component.input.length : component.input;
						flow.components.forEach(function (item) {
							if (component === item)
								return;
							Object.keys(item.connections).forEach(function (key) {
								item.connections[key] = item.connections[key].remove(function (item) {
									return item.id === component.id && (+item.index) >= count;
								});
								!item.connections[key].length && (delete component.connections[key]);
							});
						});
					}

					setTimeout(function () {
						component.options.comname = undefined;
						component.options.comreference = undefined;
						component.options.comoutput = undefined;
						component.options.cominput = undefined;
						component.options.comcolor = undefined;
						component.options.comnotes = undefined;
						//is_designer && EMIT('designer.refresh'); //TODO
					}, 300);

					//SET('common.form', '');
				},
				restore: function () {
					confirm.show('Are you sure you want to refresh this page?', ['Yes', 'Cancel'], function (index) {
						!index && location.reload(true);
					});
				},
			},

			designer: {
				zoomin: function () {
					designer.zoom(1);
				},
				zoomout: function () {
					designer.zoom(-1);
				},
				zoomreset: function () {
					designer.zoom(0);
				},
				remove: function (el) {
					el = $(el);
					if (el.hasClass('disabled')) return;

					designerComponet.remove();
				},
				unselect: function () {
					designer.select(null);
				},
				duplicate: function (el) {
					el = $(el);
					if (el.hasClass('disabled')) return;

					designerComponet.duplicate();
				},
				copy: function (el) {
					el = $(el);
					if (el.hasClass('disabled'))
						return;

					flow.clipboard = findItemById(flow.components, flow.selected.attr('data-id'));

					el.addClass('animate');
					setTimeout(function () {
						el.removeClass('animate');
					}, 500);
					on.designer.selectable(flow.selected);
				},
				paste: function (el) {
					el = $(el);
					if (el.hasClass('disabled'))
						return;

					var clone = function (component, callback) {

						var obj = Object.assign({}, component);
						var org = component.$component;
						obj.title = org.title;
						obj.output = org.output;
						obj.input = org.input;


						if (typeof (obj.state) === 'object')
							obj.state = { text: obj.state.text || org.title, color: obj.state.color || org.color };
						else
							obj.state = { text: org.title || '', color: org.color };

						loading.show();

						// copy settings from original node
						obj.x += 50;
						obj.y += 50;
						obj.connections = [];
						var desingComponent = JSON.stringify(obj);
						componentInstance.invokeMethodAsync('DuplicateNode', obj.id, desingComponent).then(function (data) {

							dupCompoment = JSON.parse(data);

							dupCompoment.$component = obj;
							dupCompoment.output = dupCompoment.output || obj.output || obj.$component.output;
							dupCompoment.input = dupCompoment.input || obj.input || obj.$component.input;

							//dupCompoment.$component = obj.$component;
							on.designer.add(dupCompoment, dupCompoment.x, dupCompoment.y, false);
							loading.hide();

						}, function (err) {
							debugger;
							loading.hide();
						});

					};

					clone(flow.clipboard, function (id) {
						flow.clipboard = null;
						on.designer.selectable(flow.selected);
					});
				},
				settings: function (el) {
					el = $(el);
					if (el.hasClass('disabled'))
						return;

					var idconnection = selected.attr('data-id');
					on.designer.settings(idconnection);
				}
			}
		};

		var websocket = {
			queue: [],
			send: function (obj) {
				websocket.queue.push(encodeURIComponent(JSON.stringify(obj)));
				websocket.process();
				return self;
			},
			process: function (callback) {

				if (!websocket.queue.length) {
					callback && callback();
					return;
				}

				var promise = new Promise(function (resolve, reject) {
					resolve(callback && callback());
				});
			},
		};

		var loadingObject,
			pointer;
		var loading = {
			make: function () {

				loadingObject = $('#loading');
				let self = loadingObject;
				self.aclass('ui-loading');
				self.append('<div></div>');
			},
			show: function () {

				let self = loadingObject;

				clearTimeout(pointer);
				self.rclass('hidden');
				return self;
			},
			hide: function (timeout) {

				let self = loadingObject;

				clearTimeout(pointer);
				pointer = setTimeout(function () {
					self.aclass('hidden');
				}, timeout || 1);
				return self;
			}
		}

		let cokiesPrefix = 'Workbench.';

		var cookie = {

			names: {
				refreshNodeStatusesCookieName: cokiesPrefix + 'refreshNodeStatuses',
				isClosedLeftMenuCookieName: cokiesPrefix + 'isClosedLeftMenu',
				isClosedRightMenuCookieName: cokiesPrefix + 'isClosedRightMenu'
			},

			createCookie: function (name, value, days) {

				days = days || 30; // 30 is default days to save cookies
				var date = new Date();
				date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
				var expires = "; expires=" + date.toGMTString();
				document.cookie = name + "=" + value + expires + "; path=/";
			},

			getCookie: function (name) {
				var nameEQ = name + "=";
				var ca = document.cookie.split(';');
				for (var i = 0; i < ca.length; i++) {
					var c = ca[i];
					while (c.charAt(0) == ' ') c = c.substring(1, c.length);
					if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
				}

				return null;
			},

			deleteCookie: function (name) {
				createCookie(name, "", -1);
			}
		}

		function getDesignerJson() {
			var data = {};
			data.tabs = [];
			//data.tabs = MESSAGE_DESIGNER.tabs;
			//data.components = flow.components;
			data.components = flow.components.map(a => Object.assign({}, a));

			data.version = 203; //TODO
			data.variables = '';
			//data.version = +exports.version.replace(/(v|\.)/g, '');
			//data.variables = FLOW.$variables;

			if (data.components) {
				data.components.forEach(function (component) {
					var tmp = [];

					if (component.connections && component.connections instanceof Array) {
						component.connections.forEach(function (connection) {
							tmp.push(connection);
						})
					}

					var res = tmp[0] === undefined ? tmp : tmp[0];
					if (!(res instanceof Array)) {
						res = tmp;
					}

					component.connections = res;
				});
			}

			var json = JSON.stringify(data, (k, v) => k === '$component' ? undefined : v);

			return json;
		};

		$(document).ready(function () {


			loading.make();
			confirm.make();

			// Init Components
			function initComponents() {

				return new Promise(function (resolve, reject) {

					jsonConfigurations = JSON.parse(designData);

					flow.isReadOnly = jsonConfigurations.isReadOnly;

					if (jsonConfigurations && jsonConfigurations.defComponents && jsonConfigurations.defComponents instanceof Array) {
						$.each(jsonConfigurations.defComponents, function (index, element) {
							common.components.push(element);
						});
					}

					if (jsonConfigurations && jsonConfigurations.defComponents) {
						createComponentMenu(jsonConfigurations.defComponents);
					} else {
						createComponentMenu();
					}

					designer.make();

					resolve(jsonConfigurations);

				}).then(function (data) {

					if (!data) data = {};

					//add flow name in tab
					$('.flowTabName').html(data.flowName);

					if (settings.isCommonMinimized.get() == 'true') {
						$('body').addClass('panel-minized');
					}

					if (settings.isMinimizeMainMenu.get() == 'true') {
						$('body').addClass('mainmenu-hidden');
					}

					if (!data.components || !data.components.length) {
						loading.hide();
						$('.ui-loading').removeClass('ui-loading-firstload');
						return;
					}

					//add external data
					$.each(data.components, function (index, item) {

						let found = $.map(common.components, function (val) {
							return val.id === item.component ? val : null;
						});

						item.$component = found[0];
						//item.name =  item.$component.title;

						if (item.$component) {
							item.$component.name = item.$component.title;
							item.$component.traffic = flow.isReadOnly !== 1;
						} else {
							item.$component = {}
							item.$component.name = item.component ?? 'Test';
							item.$component.traffic = flow.isReadOnly !== 1;
						}

						item.state = item.state || {};
						item.isnew = false;

						Object.keys(item.connections).forEach(function (index) {
							var conn = item.connections[index];
							flow.connections[item.id + '#' + index + '#' + conn.index + '#' + conn.id] = true;
						});

						flow.components.push(item);
					});

					designer.setter(data.components);

					on.resize();

					flow.loaded = true;

					loading.hide();
					$('.ui-loading').removeClass('ui-loading-firstload');

					if (flow.isReadOnly === 0) {
						$('#saveNodeUpdateChanges').remove();
						refreshTraffic();
						var ref = window.setInterval(refreshTraffic, settings.refreshNodeStatusInMilisecond.get());
					}

				});

			}

			initComponents();

			// UI style part
			$('body').attr('class', 'touch');
			$('body').attr('class', 'themedark');

			toggleMainMenu();
			toggleCommonMenu();

			//Auto collapes the navigation bar ot the left
			//$("body").toggleClass("mainmenu-hidden");
			//$(".nav-header").attr("onclick", "$('body').toggleClass('mini-navbar')")
			//$("#page-wrapper .row.border-bottom").remove();
			//$("#side-menu .nav-header .logo-element a").each(function () {
			//	$(this).removeAttr("href");
			//});

			// Color Picker
			//$('#mycp').colorpicker({ useAlpha: false }); // TODO 

			//$('.colorpicker-alpha').remove();

			//set modal designer settings on open
			//$('#workflowDesignerSettings').on('shown.bs.modal', function () {

			//	$('#isClosedLeftMenu').prop('checked', (settings.isMinimizeMainMenu.get() == 'true' ? true : false));
			//	$('#isClosedRightMenu').prop('checked', settings.isCommonMinimized.get() == 'true' ? true : false);

			//	$('#refreshNodeStatuses').val(settings.refreshNodeStatusInMilisecond.get());
			//});

			$("#nodeUpdateNameId").keypress(function (e) {
				if (e.which != 13) return;
				$('#saveNodeUpdateChanges').click();
			});

			$(window).resize(function () {
				on.resize();
			});
		});
	}

})();

