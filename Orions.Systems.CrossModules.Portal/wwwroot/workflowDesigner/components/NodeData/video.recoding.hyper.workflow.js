
var component = (function(){

    var html =
        `<div class="padding">
            <div class="row">
                <div class="col-md-12 m">
                    <div data-jc="textbox" class="m" data-jc-config="required:true;maxlength:30;placeholder:@(Name (or id) of the Workflow Phase this node belongs to. Phases allow to logically split the workflow into parts.)">WorkflowPhase</div>
                </div>
            </div>
        </div>`;

    var readme = `# VideoRecodingHyperWorkflowNodeData     `;

    return {
        id: 'videorecodinghyperworkflownodedata',
        typeFull: 'Orions.Infrastructure.HyperMedia.VideoRecodingHyperWorkflowNodeData',
        title : 'Video Recoding',
        group : 'Node Data',
        color : '#c10a11',
        input : true,
        output: 1,
        author : 'Orions',
        icon : 'commenting-o',
        html: html,
        readme: readme
    };
}());