import os
import cv2 as cv2
import numpy as np
import matplotlib.pyplot as plt
import random


from bs4 import BeautifulSoup
from util.helper import gaussian_heat_map
from util.helper import creat_roiheatmap_ellipse

class celeb_dataloader_v1:
    def __init__(self, source_path, image_width, image_height, down_sample_rate, validation_sample_rate):
        self.source_path = source_path

        self.train_label_info = []
        self.validation_label_info = []

        self.image_width = image_width
        self.image_height = image_height
        self.feature_map_width = int(image_width / down_sample_rate)
        self.feature_map_height = int(image_height / down_sample_rate)
        self.down_sample_rate = down_sample_rate
        self.label_count = 1
        self.current_train_index = 0
        self.current_validation_index = 0
        self.train_dataset_count = 0
        self.validation_dataset_count = 0
        self.validation_smaple_rate = validation_sample_rate


        temp_labelinfo = source_path + '/list_bbox_celeba.txt'
        with open(temp_labelinfo, "r" ) as file:
            content = file.readlines()
            content = " ".join(content)
            lines = content.splitlines()
            count = int(lines[0])

            self.validation_dataset_count = int(count * self.validation_smaple_rate)
            self.train_dataset_count = count - self.validation_dataset_count

            temp = []
            for index in range(0, count):
                token = lines[index + 2].split()
                info = [token[0], int(token[1]), int(token[2]), int(token[3]), int(token[4])]
                temp.append(info)

            self.train_label_info = temp[0: self.train_dataset_count]
            self.validation_label_info = temp[self.train_dataset_count:]
            print('test')







    def trainset_load(self,batch):
        images = []
        heat_maps = []
        size_maps = []
        total_box = []

        for index in range(batch):
            if index + self.current_train_index >= self.train_dataset_count:
                return (None, None, None, None)

            box_info = []

            info = self.train_label_info[index + self.current_train_index]

            image_gaussian_heat_map = np.zeros((self.feature_map_height, self.feature_map_width, self.label_count), dtype=np.float32)
            image_size_map = np.zeros((self.feature_map_height, self.feature_map_width, 2), dtype=np.float32)

            image = cv2.imread(self.source_path + '/' + info[0]).astype(np.uint8)
            image_height = image.shape[0]
            image_width = image.shape[1]
            image = cv2.resize(image, dsize=(self.image_width, self.image_height), interpolation=cv2.INTER_AREA)
            npImage = np.array(image)
            npImage = np.array(npImage, dtype=np.uint8)



            image_height_ratio = self.image_height / image_height
            image_width_ratio = self.image_width / image_width

            converted_min_x = image_width_ratio * info[1]
            converted_min_y = image_height_ratio * info[2]
            converted_width = image_width_ratio * info[3]
            converted_height = image_height_ratio * info[4]



            roi_min_x = int(converted_min_x / self.down_sample_rate)
            roi_min_y = int(converted_min_y / self.down_sample_rate)
            roi_width = int(converted_width / self.down_sample_rate)
            roi_height = int(converted_height / self.down_sample_rate)
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
            image_gaussian_heat_map[0:self.feature_map_height, 0:self.feature_map_width, 0] = np.maximum(image_gaussian_heat_map[0:self.feature_map_height, 0:self.feature_map_width, 0],temp_heat_map)

            image_size_map[size_min_y:size_max_y, size_min_x:size_max_x, 0] = roi_width
            image_size_map[size_min_y:size_max_y, size_min_x:size_max_x, 1] = roi_height
            box_info.append([roi_min_x, roi_min_y, roi_max_x, roi_max_y])


            images.append(npImage)
            heat_maps.append(image_gaussian_heat_map)
            size_maps.append(image_size_map)
            total_box.append(box_info)

            if index + self.current_train_index >= self.train_dataset_count:
                break

        self.current_train_index += batch

        return (images, heat_maps, size_maps, total_box)

    def validationset_load(self, batch):
        images = []
        heat_maps = []
        size_maps = []
        total_box = []

        for index in range(batch):
            if index + self.current_validation_index >= self.validation_dataset_count:
                return (None, None, None, None)

            box_info = []

            info = self.validation_label_info[index + self.current_validation_index]

            image_gaussian_heat_map = np.zeros((self.feature_map_height, self.feature_map_width, self.label_count), dtype=np.float32)
            image_size_map = np.zeros((self.feature_map_height, self.feature_map_width, 2), dtype=np.float32)

            image = cv2.imread(self.source_path + '/' + info[0]).astype(np.uint8)
            image_height = image.shape[0]
            image_width = image.shape[1]
            image = cv2.resize(image, dsize=(self.image_width, self.image_height), interpolation=cv2.INTER_AREA)
            npImage = np.array(image)
            npImage = np.array(npImage, dtype=np.uint8)



            image_height_ratio = self.image_height / image_height
            image_width_ratio = self.image_width / image_width

            converted_min_x = image_width_ratio * info[1]
            converted_min_y = image_height_ratio * info[2]
            converted_width = image_width_ratio * info[3]
            converted_height = image_height_ratio * info[4]



            roi_min_x = int(converted_min_x / self.down_sample_rate)
            roi_min_y = int(converted_min_y / self.down_sample_rate)
            roi_width = int(converted_width / self.down_sample_rate)
            roi_height = int(converted_height / self.down_sample_rate)
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
            image_gaussian_heat_map[0:self.feature_map_height, 0:self.feature_map_width, 0] = np.maximum(image_gaussian_heat_map[0:self.feature_map_height, 0:self.feature_map_width, 0],temp_heat_map)

            image_size_map[size_min_y:size_max_y, size_min_x:size_max_x, 0] = roi_width
            image_size_map[size_min_y:size_max_y, size_min_x:size_max_x, 1] = roi_height
            box_info.append([roi_min_x, roi_min_y, roi_max_x, roi_max_y])


            images.append(npImage)
            heat_maps.append(image_gaussian_heat_map)
            size_maps.append(image_size_map)
            total_box.append(box_info)

            if index + self.current_validation_index >= self.validation_dataset_count:
                break

        self.current_validation_index += batch

        return (images, heat_maps, size_maps, total_box)



    def train_clear(self):
        self.current_train_index = 0

    def validation_clear(self):
        self.current_validation_index = 0;

    def shuffle_train(self):
        random.shuffle(self.train_label_info)

    def shuffle_validation(self):
        random.shuffle(self.validation_label_info)

    def train_size(self):
        return self.train_dataset_count

    def validation_size(self):
        return self.validation_dataset_count
