from API_Controller import API_Controller
import csv
import os

class Model(object):
    
    def __init__(self, endpoint):
        self.controller = API_Controller()
        self.write_endpoint = endpoint

    def get_data(self, ext, params={}): return self.controller.request("GET", ext, params)    
    def post_data(self, params): pass

    def load_to_csv(self, path, json_dict):

        with open(path, "w", newline="") as file: 
            # print(os.path.abspath(path))
            writer = csv.writer(file)
            for obj in json_dict:
                writer.writerow([obj[key] for key in obj])
            file.close()

        # with open(path, "r") as file:
        #     reader = csv.reader(file)
        #     for row in reader: print(row)
        #     file.close()