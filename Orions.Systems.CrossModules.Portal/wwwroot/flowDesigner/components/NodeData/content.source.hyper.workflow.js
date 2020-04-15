
var component = (function(){

    var html =
        `<div class="padding">
            <div class="row">
                <div class="col-md-12 m">
                    <div data-jc="textbox" class="m" data-jc-config="required:true;maxlength:30;placeholder:@(Name (or id) of the Workflow Phase this node belongs to. Phases allow to logically split the workflow into parts.)">WorkflowPhase</div>
                </div>
            </div>
        </div>`;

    var readme = `# ContentSourceHyperWorkflowNodeData 

This node serves to feed HyperContent data into the Workflow for processing `;

    return {
        id: 'contentsourcehyperworkflownodedata',
        typeFull: 'Orions.Infrastructure.HyperMedia.ContentSourceHyperWorkflowNodeData',
        title : 'Content Source',
        group : 'Node Data',
        color : '#2a49c1',
        input : true,
        output: 1,
        author : 'Orions',
        icon : 'commenting-o',
        html: html,
        readme: readme
    };
}());