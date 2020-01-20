window.Orions = {};

window.Orions.JwPlayer = {
   init: function (config) {

      jwplayer.key = config.key;

      var playerSetup = {
         file: config.file,
         image: config.image,
         title: config.title,
         description: config.description,
         sources: config.sources,
         logo: config.logoConfig,
         width: config.width,
         height: config.height,
         displaytitle: config.displayTitle,
         stretching: config.stretching,
         aspectratio: config.aspectratio,
         autoplay: config.autoplay,
         preload: config.preload,
         abouttext: config.aboutText,
         aboutlink: config.aboutLink,
         autostart: config.autostart,
          mute: config.mute,
          playlist: [
              {
                  file: config.file,
                  starttime: config.startAt
              }
          ]
         //playlist: 'https://cdn.jwplayer.com/v2/playlists/qI5YMsQg?related_media_id=OpQMbAfZ',
         //related: {
         //    autoplaytimer: 10,
         //    displayMode: "shelfWidget",
         //    onclick: "link",
         //    oncomplete: "autoplay"
         //}

      };

      var playerIsntance = jwplayer(config.id);
      var player = playerIsntance.setup(playerSetup);

      player.on('ready', function () {
         player.play();
      });
   },

    seek: function (id, position) {
        jwplayer(id).seek(position);
    },

   remove: function (id) {
      var player = jwplayer(id);
      if (player !== null) {
         player.remove();
      }
   }
};

window.Orions.MetadataReviewComponent = {
	init: function() {
		document.querySelector(".metadatareview-page-controls input").addEventListener("keypress", function (evt) {
			if (evt.which != 8 && evt.which != 0 && evt.which < 48 || evt.which > 57) {
				evt.preventDefault();
			}
		});
	}
}

window.Orions.KendoTreemap = {

   init: function (id, data) {
      var treemapId = "#" + id;
      createTreeMap(treemapId, data);
      $(document).bind("kendo:skinChange", createTreeMap);
      $(".options").bind("change", refresh);

      addToolTip(treemapId);

      function createTreeMap(id, data) {
         $(id).kendoTreeMap({
            dataSource: {
               data: data,
               schema: {
                  model: {
                     children: "items"
                  }
               }
            },
            valueField: "value",
            textField: "name"

         }).on("click", ".k-leaf, .k-treemap-title", function (e) {
            var item = $(id).data("kendoTreeMap").dataItem($(this).closest(".k-treemap-tile"));
            //console.log(item.name + ": " + item.value);
         });
      }

      function addToolTip(id) {

         $(id).kendoTooltip({
            filter: ".k-leaf,.k-treemap-title",
            position: "top",
            content: function (e) {
               var treemap = $(id).data("kendoTreeMap");
               var item = treemap.dataItem(e.target.closest(".k-treemap-tile"));
               //return item.name + ": " + item.value;
               return item.value;
            }
         });
      }

      function refresh() {
         $("#treeMap").getKendoTreeMap().setOptions({
            type: $("input[name=type]:checked").val()
         });
      }

   }
};

window.Orions.KendoMediaPlayer = {

   init: function (mediaPlayerData) {
      var id = mediaPlayerData.id;
      var title = mediaPlayerData.title;
      var source = mediaPlayerData.source;

      var playerId = "#" + id;
      $(playerId).kendoMediaPlayer({
         autoPlay: mediaPlayerData.autoPlay,
         navigatable: mediaPlayerData.navigatable,
         mute: mediaPlayerData.mute,
         media: {
            title: title,
            source: source
         },
         play: function (e) {
            DotNet.invokeMethodAsync('Orions.Systems.CrossModules.Components', 'OnPlayAsync');
         },
         pause: function (e) {

            DotNet.invokeMethodAsync('Orions.Systems.CrossModules.Components', 'OnPauseAsync');

         },

         timeChange: function (e) {
            DotNet.invokeMethodAsync('Orions.Systems.CrossModules.Components', 'OnTimeChangeAsync');
         }
      });
   },

   getPlayer: function (id) {
      var playerId = "#" + id;

      return $(playerId).getKendoMediaPlayer();
   }
};

var _pickerMap = {};

window.Orions.VanillaColorPicker = {

   init: function (config, componentInstance) {

      var parent = document.querySelector('#' + config.parentId);

      var options = {
         parent: parent,
         popup: config.popup,
         alpha: config.alpha,
         editor: config.editor,
         editorFormat: config.editorFormat,
         cancelButton: config.cancelButton,
         color: config.color
      };


      var picker = new Picker(options);

      //picker.openHandler();

      picker.onDone = function (color) {
         console.log('onDone', this.settings.parent.id, color.rgba);

         var selectedColor = '';
         if (this.settings.editorFormat === 'rgb') {
            parent.style.background = this.color.rgbString;
            selectedColor = this.color.rgbString;
         }

         if (this.settings.editorFormat === 'hex') {
            parent.style.background = this.color.hex;
            selectedColor = this.color.hex;
         }

         if (this.settings.editorFormat === 'hsl') {
            parent.style.background = this.color.hslString;
            selectedColor = this.color.hslString;
         }

         parent.innerText = selectedColor;

         componentInstance.invokeMethodAsync('NotifyChange', selectedColor).then(null, function (err) {
            throw new Error(err);
         });

      };
      picker.onOpen = function (color) { console.log('Opened', this.settings.parent.id, color.rgba); };
      picker.onClose = function (color) { console.log('Closed', this.settings.parent.id, color.rgba); };
      picker.onChange = function (color) {
         //console.log('Change');
      };

      _pickerMap[config.parentId] = picker;
   },
   destroy: function () { },
   setColor: function (color) { },

};

(function () {
   window.SlideToggle = {
      init: function (componentInstance) {
         //TODO
      },
   };
})();

(function () {
   window.BlazorInputFile = {

      init: function init(elem, componentInstance) {

         var nextFileId = 0;

         elem.addEventListener('change', function handleInputFileChange(event) {
            // Reduce to purely serializable data, plus build an index by ID
            elem._blazorFilesById = {};
            var fileList = Array.prototype.map.call(elem.files, function (file) {
               var result = {
                  id: ++nextFileId,
                  lastModified: new Date(file.lastModified).toISOString(),
                  name: file.name,
                  size: file.size,
                  type: file.type
               };
               elem._blazorFilesById[result.id] = result;

               // Attach the blob data itself as a non-enumerable property so it doesn't appear in the JSON
               Object.defineProperty(result, 'blob', { value: file });

               return result;
            });

            componentInstance.invokeMethodAsync('NotifyChange', fileList).then(null, function (err) {
               throw new Error(err);
            });
         });
      },

      readFileData: function readFileData(elem, fileId, startOffset, count) {
         var readPromise = getArrayBufferFromFileAsync(elem, fileId);

         return readPromise.then(function (arrayBuffer) {
            var uint8Array = new Uint8Array(arrayBuffer, startOffset, count);
            var base64 = uint8ToBase64(uint8Array);
            return base64;
         });
      },

      ensureArrayBufferReadyForSharedMemoryInterop: function ensureArrayBufferReadyForSharedMemoryInterop(elem, fileId) {
         return getArrayBufferFromFileAsync(elem, fileId).then(function (arrayBuffer) {
            getFileById(elem, fileId).arrayBuffer = arrayBuffer;
         });
      },

      readFileDataSharedMemory: function readFileDataSharedMemory(readRequest) {
         // This uses various unsupported internal APIs. Beware that if you also use them,
         // your code could become broken by any update.
         var inputFileElementReferenceId = Blazor.platform.readStringField(readRequest, 0);
         var inputFileElement = document.querySelector('[_bl_' + inputFileElementReferenceId + ']');
         var fileId = Blazor.platform.readInt32Field(readRequest, 4);
         var sourceOffset = Blazor.platform.readUint64Field(readRequest, 8);
         var destination = Blazor.platform.readInt32Field(readRequest, 16);
         var destinationOffset = Blazor.platform.readInt32Field(readRequest, 20);
         var maxBytes = Blazor.platform.readInt32Field(readRequest, 24);

         var sourceArrayBuffer = getFileById(inputFileElement, fileId).arrayBuffer;
         var bytesToRead = Math.min(maxBytes, sourceArrayBuffer.byteLength - sourceOffset);
         var sourceUint8Array = new Uint8Array(sourceArrayBuffer, sourceOffset, bytesToRead);

         var destinationUint8Array = Blazor.platform.toUint8Array(destination);
         destinationUint8Array.set(sourceUint8Array, destinationOffset);

         return bytesToRead;
      }
   };

   function getFileById(elem, fileId) {
      var file = elem._blazorFilesById[fileId];
      if (!file) {
         throw new Error('There is no file with ID ' + fileId + '. The file list may have changed');
      }

      return file;
   }

   function getArrayBufferFromFileAsync(elem, fileId) {
      var file = getFileById(elem, fileId);

      // On the first read, convert the FileReader into a Promise<ArrayBuffer>
      if (!file.readPromise) {
         file.readPromise = new Promise(function (resolve, reject) {
            var reader = new FileReader();
            reader.onload = function () { resolve(reader.result); };
            reader.onerror = function (err) { reject(err); };
            reader.readAsArrayBuffer(file.blob);
         });
      }

      return file.readPromise;
   }

   var uint8ToBase64 = (function () {
      // Code from https://github.com/beatgammit/base64-js/
      // License: MIT
      var lookup = [];

      var code = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
      for (var i = 0, len = code.length; i < len; ++i) {
         lookup[i] = code[i];
      }

      function tripletToBase64(num) {
         return lookup[num >> 18 & 0x3F] +
            lookup[num >> 12 & 0x3F] +
            lookup[num >> 6 & 0x3F] +
            lookup[num & 0x3F];
      }

      function encodeChunk(uint8, start, end) {
         var tmp;
         var output = [];
         for (var i = start; i < end; i += 3) {
            tmp =
               ((uint8[i] << 16) & 0xFF0000) +
               ((uint8[i + 1] << 8) & 0xFF00) +
               (uint8[i + 2] & 0xFF);
            output.push(tripletToBase64(tmp));
         }
         return output.join('');
      }

      return function fromByteArray(uint8) {
         var tmp;
         var len = uint8.length;
         var extraBytes = len % 3; // if we have 1 byte left, pad 2 bytes
         var parts = [];
         var maxChunkLength = 16383; // must be multiple of 3

         // go through the array every three bytes, we'll deal with trailing stuff later
         for (var i = 0, len2 = len - extraBytes; i < len2; i += maxChunkLength) {
            parts.push(encodeChunk(
               uint8, i, (i + maxChunkLength) > len2 ? len2 : (i + maxChunkLength)
            ));
         }

         // pad the end with zeros, but make sure to not forget the extra bytes
         if (extraBytes === 1) {
            tmp = uint8[len - 1];
            parts.push(
               lookup[tmp >> 2] +
               lookup[(tmp << 4) & 0x3F] +
               '=='
            );
         } else if (extraBytes === 2) {
            tmp = (uint8[len - 2] << 8) + uint8[len - 1];
            parts.push(
               lookup[tmp >> 10] +
               lookup[(tmp >> 4) & 0x3F] +
               lookup[(tmp << 2) & 0x3F] +
               '='
            );
         }

         return parts.join('');
      };
   })();
})();


function raiseResizeEvent() {
   var namespace = 'Orions.Systems.CrossModules.Components'; // the namespace of the app
   var method = 'RaiseWindowResizeEvent'; //the name of the method in our "service"
   DotNet.invokeMethodAsync(namespace, method, Math.floor(window.innerWidth), Math.floor(window.innerHeight));
}

//throttle resize event, taken from https://stackoverflow.com/a/668185/812369
var timeout = false;
window.addEventListener("resize", function () {
   if (timeout !== false)
      clearTimeout(timeout);

   timeout = setTimeout(raiseResizeEvent, 200);
});


(function () {
   window.Orions.MapZone = {
      Init: function (elementId, classNameOnMouseOver, componentInstance) {
         var element = document.querySelector('#' + elementId);

         if (element && element.hasChildNodes()) {
            NodeList.prototype.forEach = Array.prototype.forEach;
            var zones = element.childNodes;

            zones.forEach(function (zone) {
               if (zone.nodeType !== Node.TEXT_NODE) {
                              
                  zone.addEventListener("mouseover", function (event) {
                     event.target.classList.add(classNameOnMouseOver);
                  });

                  zone.addEventListener("mouseout", function (event) {
                     event.target.classList.remove(classNameOnMouseOver);
                  });

                  zone.addEventListener("click", function (event) {
                     componentInstance.invokeMethodAsync('OnZoneClick', zone.id).then(null, function (err) {
                        throw new Error(err);
                     });
                  });
               }
            });
         }
      },
      AddClassById: function (elementId, className) {
         var element = document.querySelector('#' + elementId);

         element.className = className;
         element.classList.add(className);
      },
      RemoveClassById: function (elementId, className) {
         var element = document.querySelector('#' + elementId);

         element.className = className;
         element.classList.remove(className);
      }
   };
})();