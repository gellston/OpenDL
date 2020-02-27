import os
import cv2 as cv2
import numpy as np
import matplotlib.pyplot as plt
import random


from bs4 import BeautifulSoup
from util.helper import gaussian_heat_map
from util.helper import creat_roiheatmap_ellipse

class objectdetection_dataloader_v1:
    def __init__(self, source_path, image_width, image_height, down_sample_rate):
        self.source_path = source_path

        self.label_info = []
        self.xml_files = []
        self.image_info = []

        self.image_width = image_width
        self.image_height = image_height
        self.feature_map_width = int(image_width / 4)
        self.feature_map_height = int(image_height / 4)
        self.down_sample_rate = down_sample_rate
        self.label_count = 0
        self.currentIndex = 0
        self.dataset_count = 0

        temp_labelinfo = source_path + '/___LabelInfo.xml'
        with open(temp_labelinfo, "r" , encoding="utf-8") as file:
            content = file.readlines()
            content = "".join(content)
            bs_content = BeautifulSoup(content, "xml" , from_encoding='utf-8')
            vtroiArray = bs_content.ArrayOfVTROI
            vtroi = vtroiArray.findAll('VTROI')
            self.label_count = len(vtroi)
            for roi in vtroi:
                self.label_info.append(roi.Name.string)

        filelist = sorted(os.listdir(self.source_path ))
        for filename in filelist:
            if filename == '.DS_Store': continue
            if filename == '___LabelInfo.xml' : continue
            if filename.endswith('jpg') : continue

            xmlPath = self.source_path + '/' + filename
            self.xml_files.append(xmlPath)
            self.dataset_count = self.dataset_count + 1


    def load(self,batch):
        images = []
        heat_maps = []
        size_maps = []
        total_box = []

        for index in range(batch):
            if index + self.currentIndex >= self.dataset_count:
                return (None, None, None, None)

            xml_path = self.xml_files[self.currentIndex + index]
            with open(xml_path, "r", encoding="utf-8") as file:
                content = file.readlines()
                content = "".join(content)
                bs_content = BeautifulSoup(content, "xml", from_encoding='utf-8')


            detectionInfo = bs_content.VTObjectDetectionInfo
            filePath = self.source_path + '/' + detectionInfo.FileName.string
            #print('file Path = ' , filePath)
            vtroi = detectionInfo.RoiItems.findAll('VTROIRect')

            image_gaussian_heat_map = np.zeros((self.feature_map_height, self.feature_map_width, self.label_count), dtype=np.float32)
            image_size_map = np.zeros((self.feature_map_height, self.feature_map_width, 2), dtype=np.float32)

            box_info = []
            for roi in vtroi:

                roi_name = roi.Name.string
                feature_index = self.label_info.index(roi_name)
                roi_min_x = int(float(roi.X.string) / self.down_sample_rate)
                roi_min_y = int(float(roi.Y.string) / self.down_sample_rate)
                roi_width = int(float(roi.Width.string) / self.down_sample_rate)
                roi_height = int(float(roi.Height.string) / self.down_sample_rate)
                roi_max_x = roi_min_x + roi_width
                roi_max_y = roi_min_y + roi_height
                roi_center_x = int(roi_min_x + roi_width / 2)
                roi_center_y = int(roi_min_y + roi_height / 2)
                size_min_x = roi_center_x - 1
                size_max_x = roi_center_x + 1
                size_min_y = roi_center_y - 1
                size_max_y = roi_center_y + 1


                temp_heat_map = gaussian_heat_map(self.feature_map_width, self.feature_map_height,
                                                  roi_center_x, roi_center_y,
                                                  roi_width, roi_height)
                temp_heat_max = np.max(temp_heat_map)
                temp_heat_map = temp_heat_map / temp_heat_max
                image_gaussian_heat_map[0:self.feature_map_height, 0:self.feature_map_width, feature_index] = np.maximum(image_gaussian_heat_map[0:self.feature_map_height, 0:self.feature_map_width, feature_index], temp_heat_map)

                image_size_map[size_min_y:size_max_y, size_min_x:size_max_x, 0] = roi_width
                image_size_map[size_min_y:size_max_y, size_min_x:size_max_x, 1] = roi_height
                box_info.append([roi_min_x, roi_min_y, roi_max_x, roi_max_y])



            image = cv2.imread(filePath).astype(np.uint8)
            npImage = np.array(image)
            npImage = np.array(npImage, dtype=np.uint8)

            images.append(npImage)
            heat_maps.append(image_gaussian_heat_map)
            size_maps.append(image_size_map)
            total_box.append(box_info)

            if index + self.currentIndex >= self.dataset_count:
                break

        self.currentIndex += batch

        return (images, heat_maps, size_maps, total_box)

    def clear(self):
        self.currentIndex = 0

    def shuffle(self):
        random.shuffle(self.xml_files)

    def size(self):
        return self.dataset_count