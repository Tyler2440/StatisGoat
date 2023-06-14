import http.client
import json

class API_Controller(object):
    
    def __init__(self):
        self.conn = http.client.HTTPConnection("localhost:5000")
        self.base_endpoint = "/api/"

    def request(self, method, ext, params): 
        req = self.base_endpoint + ext
        if len(params) > 0: req += "?" + "&".join([param + "=" + str(params[param]) for param in params])

        self.conn.request(method, req)

        # can access json values using properties of return value
        return json.loads(self.conn.getresponse().read().decode("utf-8"))
