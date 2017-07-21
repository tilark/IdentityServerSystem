// Write your Javascript code.
$(function () {
    console.log("Ready!");

});
//删除用户UserClaim
function deleteUserClaimSuccess(data, status, jqXHR)
{
    if (data.success)
    {
        this.remove();
    }
    else {
        console.log("删除失败!");
    }
}
function addClaimType(event) {
    //console.log(this);
    var claimType = $(".identityResourceClaimType").first();
    claimType.first().clone().insertBefore("#add-claim-type");
}
function addCloneElementToTargetElementID(event, cloneElement, insertToElementID) {
    //console.log(this);
    var claimType = $(cloneElement).first();
    claimType.first().clone().insertBefore(insertToElementID);
}
function deleteClaimType(element){
    element.parentsUntil(".identityResourceClaimType").parent().remove();
   
}
function deleteTargetElement(element, targetElement) {
    element.parentsUntil(targetElement).parent().remove();

}

function bindSimpleSelect2() {
    $(".multiple-bind-select2").select2({
        language: 'zh-CN'
    });
}
function getIdentityResourceSelect(element) {
    var data = {};
    data.id = element;
    $("#target-identity-resource-claims").load("/ManageIdentityResources/GetClaimTypeByIdentityResourceID", data);
}