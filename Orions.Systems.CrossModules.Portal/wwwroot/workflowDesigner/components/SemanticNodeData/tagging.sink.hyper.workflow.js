
var component = (function(){

    var html =
        `<div class="padding">
            <div class="row">
                <div class="col-md-12 m">
                    <div data-jc="textbox" class="m" data-jc-config="required:true;maxlength:30;placeholder:@(Name (or id) of the Workflow Phase this node belongs to. Phases allow to logically split the workflow into parts.)">WorkflowPhase</div>
                </div>
            </div>
        </div>`;

    var readme =  `# TaggingSinkHyperWorkflowNodeData    

Serialized via a Memento. `;

    return {
        id: 'taggingsinkhyperworkflownodedata',
        typeFull: 'Orions.Infrastructure.HyperSemantic.TaggingSinkHyperWorkflowNodeData',
        title : 'Tagging Sink',
        group : 'Semantic Node Data',
        color : '#2a49c1',
        input : true,
        output: 1,
        author : 'Orions',
        icon : 'commenting-o',
        html: html,
        readme: readme
    };
}());