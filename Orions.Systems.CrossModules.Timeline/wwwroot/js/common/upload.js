(function ($) {
    $(".file").change(function () {
        var autoChange = $(this).data('auto-upload');
        if (!autoChange) {
            return;
        } else {
            $(this).upload();
        }
    });
    $.fn.setup = function () {
        var fileControl = $(this);
        var uploadControl = fileControl.closest(".uploadControl");
        var progress = uploadControl.find(".progress");
        uploadControl.find("#UploadId").val("");
        fileControl.val("");
        if (!progress.hasClass("hidden")) {
            progress.addClass("hidden")
        }
    }
    $.fn.upload = function (clientProcess, clientSuccess, clientError) {
        fileUpload = this[0];
        if (fileUpload == null) {
            error("Undefined files");
            return;
        }
        if (!fileUpload.files.length) {
            error("Missing files");
            return;
        }
        var fileReader =
            new FileReader(),
            file = fileUpload.files[0];

        var fileControl = $(this);
        var accept = fileControl.attr("accept").match(/^.+\.([^.]+)$/)[1];        
        var ext = fileUpload.value.match(/^.+\.([^.]+)$/)[1];
        if (accept.toLocaleLowerCase() != ext.toLocaleLowerCase()) {
            if (accept != "" && accept != "*.*" && accept != ".*") {
                clientError("Only zip files are allowed");
                return;
            }
        }

        var url = this.data('request-url');
        var id = guid();
        var fileControl = $(this);
        var uploadControl = fileControl.closest(".uploadControl");
        var progress = uploadControl.find(".progress");
        $(this).attr("disabled", true);

        fileReader.onload = function (e) {
            loadFile(
                url, id, new Uint8Array(e.target.result),
                function (value) {
                    if (value == 0) {
                        progress.removeClass("hidden");
                    }
                    progress.find(".progress-bar").css('width', value + "%");
                    progress.find(".progress-bar-message").text(value + "%");
                    if (!isNaN(clientProcess)) {
                        clientProcess(value);
                    }
                },
                function (id) {
                    progress.find(".progress-bar-message").text("File was uploaded");
                    uploadControl.find("#UploadId").val(id);
                    fileControl.attr("disabled", false);
                    clientSuccess(id);
                },
                function (result) {
                    progress.find(".progress-bar-message").text(result);
                    clientError(result);
                });
        };
        fileReader.readAsArrayBuffer(file);
    };
    function loadFile(url, uploadId, buffer, process, success, error) {
        var formData, blob;
        var index = 0;
        var step = 0;
        var partSize = 1024 * 1024; //1Mb
        var steps = Math.ceil(buffer.length / partSize);
        var progress = 0;
        process(0);
        for (var i = 0; i < buffer.length; i += partSize) {
            blob = new Blob([buffer.subarray(i, i + partSize)]);
            formData = new FormData();
            formData.append("fileUpload", blob);
            $.ajax({
                url: url + "?id="+ uploadId + "&index=" + index,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response, status, result) {
                    step++;
                    console.log(step + " : "+ ((1 - (steps - step) / steps) * 100));
                    progress = Math.max(Math.round((1 - (steps - step) / steps) * 100), progress);
                    process(progress);
                    if (step >= steps) {
                        success(uploadId);
                    }
                },
                error: function (response, status, result) {
                    error(result);
                }
            });
            index++;
        }
    }
})(jQuery);
