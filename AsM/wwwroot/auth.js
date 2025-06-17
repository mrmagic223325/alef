export function SignIn(dotnet, account, password)
{
    var url = "/api/auth/signin";
    var xhr = new XMLHttpRequest();
    
    xhr.open("POST",  url);
    xhr.setRequestHeader("Accept", "application/json");
    xhr.setRequestHeader("Content-Type", "application/json");
    
    xhr.onload = function () {
        if (xhr.status === 200)
        {
            // Success
            dotnet.invokeMethodAsync('ValidationErrors', "");
        } else
        {
            dotnet.invokeMethodAsync('ValidationErrors', xhr.responseText);
        }
    };
    
    xhr.onerror = function ()  {
        dotnet.invokeMethodAsync('ValidationErrors', 'A network error occurred. Please try again.')
    };
    
    var data = {
        account: account,
        password: password,
        };
    
    xhr.send(JSON.stringify(data));
}

export function SignOut(redirect)
{
    var url = "api/auth/signout";
    var xhr = new XMLHttpRequest();
    
    xhr.open("POST", url);
    xhr.setRequestHeader("Accept", "application/json");
    xhr.setRequestHeader("Content-Type", "application/json");
    
    xhr.onreadystatechange = function ()
    {
        if (xhr.readyState === 4)
        {
            console.log("Call '" + url + "'. Status " + xhr.status);
            if (redirect)
                location.replace(redirect);
        }
    };
    xhr.send();
}