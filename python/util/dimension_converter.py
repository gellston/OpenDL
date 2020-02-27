import os
import cv2 as cv2
import numpy as np



source_path = 'C://Github//Dataset//celebrity_dataset_bbox//img_celeba'
target_path = 'C://Github//Dataset//celebrity_dataset_bbox//img_celeba_resize512x512'

target_image_width = 512
target_image_height = 512


filelist = sorted(os.listdir(source_path))
for filename in filelist:
    if filename == '.DS_Store': continue
    if filename.endswith('jpg')  == False: continue

    image = cv2.imread(source_path + '//' + filename)
    resize = cv2.resize(image, dsize=(target_image_width, target_image_height), interpolation=cv2.INTER_AREA)
    target_save_path = target_path + '//' + filename
    cv2.imwrite(target_save_path, resize)
