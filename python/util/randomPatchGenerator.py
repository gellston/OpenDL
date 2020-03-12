import os
import cv2 as cv2
import numpy as np
import random

class randomPatchGenerator:
    def __init__(self, image_path, output_path):
        self.input_images_path = image_path
        self.output_path = output_path

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

    def shuffle(self):
        random.shuffle(self.input_images_paths)


    def generator(self, color=0, width=256, height=256, shape=[256, 256, 1], resizeWidth=256, resizeHeight=256, randomSample=100):
        #images = []

        #random.shuffle(self.input_images_paths)

        for index in range(len(self.input_images_paths)):
            #if index + self.currentIndex >= self.dataset_count:
            #    return (None, None)


            if color == 1:
                self.input_color = cv2.IMREAD_COLOR
            else:
                self.input_color = cv2.IMREAD_GRAYSCALE

            image_path = self.input_images_paths[index]

            image = cv2.imread(image_path, self.input_color).astype(np.uint8)
            cv_image = cv2.resize(image, dsize=(resizeWidth, resizeHeight), interpolation=cv2.INTER_AREA)

            for ramdomIndex in range(randomSample):
                    random_X = random.randrange(0, resizeWidth - width)
                    random_Y = random.randrange(0, resizeHeight - height)

                    randomWidth = random_X + width
                    randomHeight = random_Y + height

                    crop_image = cv_image[random_X:randomWidth, random_Y:randomHeight]
                    npImage = np.array(crop_image)
                    npImage = npImage.reshape(shape)

                    cv2.imwrite(self.output_path + '/' + str(index) + "_" + str(ramdomIndex) + '.jpg', npImage)

                    #npImage = np.array(npImage, dtype=np.float)
                    #images.append(npImage)





    def clear(self):
        self.currentIndex = 0

    def size(self):
        return self.dataset_count

