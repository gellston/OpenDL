import tensorflow as tf
import matplotlib.pyplot as plt
import cv2 as cv2
import numpy as np

from model.model_objectdetection_ppm_centernet_v1 import model_objectdetection_ppm_centernet_v1
from util.celeb_dataloader_v1 import celeb_dataloader_v1
from util.helper import extract_box_from_featuremaps
from util.helper import filter_overlap_boxes
from util.helper import mAP_calculation

dataloader = celeb_dataloader_v1('D:/LWF_CELEBA/2000_subset', 512, 512, 4, 0.25)

train_epoch = 10000
batch_size = 10
target_accuracy = 0.85
learning_rate = 0.003
iteration_check = 10

sess = tf.Session()
model = model_objectdetection_ppm_centernet_v1(sess=sess, class_count=1)
saver = tf.compat.v1.train.Saver()

#writer = tf.summary.FileWriter("C:/Github/VTDEEP/python/pretrained-model/model_objectdetection_centernet/tensorboard", sess.graph)
#writer.close()


### gloal variable initializer
global_initializer = tf.compat.v1.global_variables_initializer()
print("global_initializer = ", global_initializer.name)
sess.run(global_initializer)

train_cost_graph = []
train_accuracy_graph = []
validation_cost_graph = []
validation_accuracy_graph = []

print('Learning start.')

plt.ion()
fig, axis = plt.subplots(2, 4)
DPI = fig.get_dpi()
fig.set_size_inches(1280/float(DPI),768/float(DPI))

axis[0, 0].set_title('input image')
axis[0, 1].set_title('input label')
axis[0, 2].set_title('heatmap')

axis[1, 0].set_title('train_accuracy')
axis[1, 0].set_xlabel('epoch')
axis[1, 0].set_ylabel('train_accuracy')

axis[1, 1].set_title('train_cost')
axis[1, 1].set_xlabel('epoch')
axis[1, 1].set_ylabel('train_cost')

axis[1, 2].set_title('validation_accuracy')
axis[1, 2].set_xlabel('epoch')
axis[1, 2].set_ylabel('validation_accuracy')

axis[1, 3].set_title('validation_cost')
axis[1, 3].set_xlabel('epoch')
axis[1, 3].set_ylabel('validation_cost')

#cv2.namedWindow('heatmap', cv2.WINDOW_NORMAL)
#cv2.resizeWindow('heatmap', 512, 512)

#cv2.namedWindow('image', cv2.WINDOW_NORMAL)
#cv2.resizeWindow('image', 512, 512)

for train_step in range(train_epoch):
    dataloader.train_clear()
    dataloader.shuffle_train()

    batch_step = int(dataloader.train_size() / batch_size)

    train_avg_cost = 0
    train_avg_accuracy = 0

    print('Maximum batch step = ', batch_step)

    iteration = 0
    for batch in range(batch_step):
        iteration += 1
        train_images, train_heatmpas, train_sizes, train_boxes = dataloader.trainset_load(batch_size)

        if train_images is None:
            dataloader.train_clear()
            break

        model.train(train_images, train_heatmpas, train_sizes, learn_rate=learning_rate, keep_prop=True)
        cost = model.get_cost(train_images, train_heatmpas, train_sizes, keep_prop=False)
        output_heatmap, output_sizes = model.predict(train_images, keep_prop=False)
        train_avg_cost += cost / batch_step

        for index in range(batch_size):
            box_list = extract_box_from_featuremaps(128, 128, 1, output_heatmap[index], output_sizes[index], 0.5)
            train_avg_accuracy += (mAP_calculation(train_boxes[index], box_list, 0.5) / batch_size / batch_step)

        if iteration == iteration_check:
            iteration = 0
            print('Intermidiate Check !! Epoch : ', '%04d' % (train_step + 1), 'train_cost =', '{:.9f}'.format(train_avg_cost), 'train_accuracy =','{:.9f}'.format(train_avg_accuracy))



    print('1 epoch done')
    #saver.save(sess, 'C:/Github/VTDEEP/python/pretrained-model/model_objectdetection_atrous_centernet(celba_dataloader)/VTCenternet')
    #tf.train.write_graph(sess.graph_def, "","C:/Github/VTDEEP/python/pretrained-model/model_objectdetection_atrous_centernet(celba_dataloader)/VTCenternet.pbtxt",as_text=True)

    valid_acc = 0
    valid_cost = 0
    validation_avg_cost = 0
    validation_avg_accuracy = 0
    dataloader.validation_clear()
    dataloader.shuffle_validation()
    validation_sample_size = dataloader.validation_size()
    for validation_step in range(validation_sample_size):

        validation_images, validation_heatmaps, validation_size_map, validation_box_infos = dataloader.validationset_load(1)

        output_heatmap, output_sizes = model.predict(validation_images, keep_prop=False)
        cost = model.get_cost(validation_images, validation_heatmaps, validation_size_map, keep_prop=False)
        validation_avg_cost += (cost / validation_sample_size)

        box_list = extract_box_from_featuremaps(128, 128, 1, output_heatmap[0], output_sizes[0], 0.5)
        validation_avg_accuracy += (mAP_calculation(validation_box_infos[0], box_list, 0.5) / validation_sample_size)

        #print('Intermidiate Check Validation : ', '%04d' % (train_step + 1),'valid_cost =', '{:.9f}'.format(validation_avg_cost), 'valid_accuracy =', '{:.9f}'.format(validation_avg_accuracy))

        box_list = extract_box_from_featuremaps(128, 128, 4, output_heatmap[0], output_sizes[0], 0.5)
        for box in box_list:
            cv2.rectangle(validation_images[0], (box[0], box[1]), (box[2], box[3]), (0, 0, 255), 3)

    train_cost_graph.append(train_avg_cost)
    train_accuracy_graph.append(train_avg_accuracy)

    validation_cost_graph.append(validation_avg_cost)
    validation_accuracy_graph.append(validation_avg_accuracy)

    #cv2.imshow('image', np.reshape(validation_images[0], (512, 512, 3)))
    #cv2.imshow('heatmap', np.reshape(validation_heatmaps[0], (128, 128)))
    #cv2.waitKey(0)

    axis[0 ,0].imshow(cv2.cvtColor(validation_images[0], cv2.COLOR_BGR2RGB))
    axis[0, 1].imshow(cv2.cvtColor(validation_heatmaps[0], cv2.COLOR_GRAY2RGB))
    axis[0, 2].imshow(np.reshape(output_heatmap[0], (128, 128)))
    axis[1, 0].plot(train_accuracy_graph)
    axis[1, 1].plot(train_cost_graph)
    axis[1, 2].plot(validation_accuracy_graph)
    axis[1, 3].plot(validation_cost_graph)

    plt.draw()
    plt.pause(0.001)


    print('Epoch : ', '%04d' % (train_step + 1), 'train_cost =', '{:.9f}'.format(train_avg_cost), 'train_accuracy =', '{:.9f}'.format(train_avg_accuracy), 'valid_cost =', '{:.9f}'.format(validation_avg_cost), 'valid_accuracy =', '{:.9f}'.format(validation_avg_accuracy))
    if validation_avg_accuracy > target_accuracy:
        break;

saver.save(sess, 'C:/Github/VTDEEP/python/pretrained-model/model_objectdetection_atrous_centernet(celba_dataloader)/VTCenternet')
tf.train.write_graph(sess.graph_def, "", "C:/Github/VTDEEP/python/pretrained-model/model_objectdetection_atrous_centernet(celba_dataloader)/VTCenternet.pbtxt", as_text=True)


print('Learning finished.')