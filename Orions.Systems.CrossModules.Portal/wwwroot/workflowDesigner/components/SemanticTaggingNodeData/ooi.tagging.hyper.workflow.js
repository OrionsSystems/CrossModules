
var component = (function(){

    var html =
        `<div class="padding">
            <div class="row">
                <div class="col-md-12 m">
                    <div data-jc="textbox" class="m" data-jc-config="required:true;maxlength:30;placeholder:@(Name (or id) of the Workflow Phase this node belongs to. Phases allow to logically split the workflow into parts.)">WorkflowPhase</div>
                </div>
            </div>
        </div>`;

    var readme = `# OOITaggingHyperWorkflowNodeData  

The data for the OOI tagging node, where the vast majority of our manual tagging operations are performed. `;

    return {
        id: 'ooitagginghyperworkflownodedata',
        typeFull: 'Orions.Infrastructure.HyperSemantic.OOITaggingHyperWorkflowNodeData',
        title : 'OOI Tagging',
        group : 'Semantic Tagging Node Data',
        color : '#e09113',
        input : true,
        output: 1,
        author : 'Orions',
        icon : 'commenting-o',
        html: html,
        readme: readme
    };
}());