export async function ChangeSettings(type, data) 
{
    return new Promise((resolve, reject) => {
        const url = "/api/settings/change";
        const xhr = new XMLHttpRequest();    
    
        xhr.open("POST", url);
        
        xhr.setRequestHeader("Content-Type", "application/json");
        
        xhr.onload = () => {
            if (xhr.status >= 200 && xhr.status < 300)
            {
                try
                {
                    resolve(true);
                }
                catch (e)
                {
                    console.error(e);
                    reject(e);
                }
            }
            else
            {
                console.error(xhr.statusText);
                reject(new Error(`HTTP Error ${xhr.status}: ${xhr.statusText}`));
            }
        };

        xhr.onerror = () => {
            console.error("Network Error");
            reject(new Error("Network Error"));
        };
        
        xhr.send(JSON.stringify({ type, data }));
    });
}
