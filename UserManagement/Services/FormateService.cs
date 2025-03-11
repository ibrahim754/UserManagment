using ErrorOr;
 using UserManagement.Interfaces;

namespace UserManagement.Services
{
    public class FormateService:IFormateService
    {
        public ErrorOr<string> GenerateHtmlBody(string displayName, string content)
        {

            return $@"
                <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px; }}
                            .header {{ text-align: center; padding: 10px; background-color: #f8f9fa; border-bottom: 2px solid #e9ecef; }}
                            .content {{ padding: 30px 20px; }}
                            .footer {{ text-align: center; padding: 20px; background-color: #f8f9fa; border-top: 2px solid #e9ecef; margin-top: 30px; }}
                            h1 {{ color: #007bff; margin: 0; }}
                            p {{ margin: 15px 0; }}
                            .highlight {{ color: #28a745; font-weight: bold; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>{displayName}</h1>
                            </div>
                            <div class='content'>
                                {content}
                            </div>
                            <div class='footer'>
                                <p style='color: #6c757d; margin: 0;'>
                                    © {DateTime.Now.Month} {DateTime.Now.Year}  {displayName}. All rights reserved.
                                </p>
                            </div>
                        </div>
                    </body>
                </html>";
        }
    }
}
