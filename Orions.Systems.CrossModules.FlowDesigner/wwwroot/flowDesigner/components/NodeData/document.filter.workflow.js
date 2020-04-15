
var component = (function(){

    var html =
        `<div class="padding">
            <div class="row">
                <div class="col-md-12 m">
                    <div data-jc="textbox" class="m" data-jc-config="required:true;maxlength:30;placeholder:@(Name (or id) of the Workflow Phase this node belongs to. Phases allow to logically split the workflow into parts.)">WorkflowPhase</div>
                </div>
            </div>
        </div>`;

    var readme = `# DocumentFilterWorkflowNodeData  

Filter tasks based on queries to the document system. `;

    return {
        id: 'documentfilterworkflownodedata',
        typeFull: 'Orions.Infrastructure.HyperMedia.DocumentFilterWorkflowNodeData',
        title : 'Document Filter',
        group : 'Node Data',
        color : '#8CC152',
        input : true,
        output: 2,
        author : 'Orions',
        icon : 'commenting-o',
        html: html,
        readme: readme
    };
}());