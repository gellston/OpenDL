import os
import cv2 as cv2
import numpy as np


class anogan_dataloader:
    def __init__(self, image_path):
        self.input_images_path = image_path

        self.input_color = cv2.IMREAD_COLOR
        self.input_images_paths = []
        self.dataset_count = 0
        self.currentIndex = 0

        filelist = sorted(os.listdir(self.input_images_path))
        for filename in filelist:
            if filename == '.DS_Store': continue
            temp1 = self.input_images_path + '/' + filename
            self.input_images_paths.append(temp1)
            self.dataset_count += 1



    def load(self, color=cv2.IMREAD_GRAYSCALE, width=256, height=256, shape=[256, 256, 1], batch=5):
        images = []


        for index in range(batch):
            if index + self.currentIndex >= self.dataset_count:
                return (None, None)


            if color == 1:
                self.input_color = cv2.IMREAD_COLOR
            else:
                self.input_color = cv2.IMREAD_GRAYSCALE

            image_path = self.input_images_paths[self.currentIndex + index]

            image = cv2.imread(image_path, self.input_color).astype(np.uint8)
            cv_image = cv2.resize(image, dsize=(width, height), interpolation=cv2.INTER_AREA)
            npImage = np.array(cv_image)
            npImage = npImage.reshape(shape) / 255
            #npImage = np.array(npImage, dtype=np.float)
            images.append(npImage)

            if index + self.currentIndex >= self.dataset_count:
                break

        self.currentIndex += batch

        return images

    def clear(self):
        self.currentIndex = 0

    def size(self):
        return self.dataset_count

