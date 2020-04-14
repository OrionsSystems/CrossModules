
var component = (function(){

    var html =
        `<div class="padding">
            <div class="row">
                <div class="col-md-12 m">
                    <div data-jc="textbox" class="m" data-jc-config="required:true;maxlength:30;placeholder:@(Name (or id) of the Workflow Phase this node belongs to. Phases allow to logically split the workflow into parts.)">WorkflowPhase</div>
                </div>
            </div>
        </div>`;

    var readme = `# MLTaggingHyperWorkflowNodeData

Basic Machine Learning focused data analysis.
This data is serviced by the TaggingHyperWorkflowNode, much like all the Tagging Data classes. `;

    return {
        id: 'mltagginghyperworkflownodedata',
        typeFull: 'Orions.Infrastructure.HyperSemantic.MLTaggingHyperWorkflowNodeData',
        title : 'ML Tagging',
        group : 'Semantic ML Node Data',
        color : '#8CC152',
        input : true,
        output: 1,
        author : 'Orions',
        icon : 'commenting-o',
        html: html,
        readme: readme
    };
}());